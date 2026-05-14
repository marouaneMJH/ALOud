# Expert System — Full Pipeline Documentation

This document explains the complete flow from a user filling the recommendation form to receiving perfume recommendations with images. Every step explains both **what** happens and **why** it was designed that way.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Phase 0 — RAG Indexing Pipeline](#2-phase-0--rag-indexing-pipeline)
3. [Phase 1 — User Form → UserProfile](#3-phase-1--user-form--userprofile)
4. [Phase 2 — NRules Engine → Recommendation](#4-phase-2--nrules-engine--recommendation)
5. [Phase 3 — Vector Search → Top-5 Perfumes](#5-phase-3--vector-search--top-5-perfumes)
6. [Phase 4 — LLM Generation → Final Answer](#6-phase-4--llm-generation--final-answer)
7. [Phase 5 — Response to the View](#7-phase-5--response-to-the-view)

---

## 1. Architecture Overview

The system is a **Hybrid Expert System**: a classical rule engine (NRules) that understands perfumery domain knowledge, combined with semantic vector search (Qdrant + Ollama embeddings) and an LLM (Gemini / Groq / Grok) that generates the human-readable recommendation.

```
                         ┌──────────────────────────────────────┐
                         │  PHASE 0  (once on app startup)      │
                         │                                      │
  SQL Server             │  ProductDataExtractor                │
  Perfumes ──────────────►  ↓ PerfumeRagSource                 │
  + Brands               │  DocumentBuilderService              │
  + Notes                │  ↓ plain-text document               │
  + Accords              │  ChunkingService                     │
  + Families             │  ↓ RagDocumentChunk[]               │
  + Seasons              │  OllamaEmbeddingClient               │
  + Occasions            │  (nomic-embed-text, 768 dims)        │
  + Tags                 │  ↓ float[768] per chunk              │
                         │  QdrantVectorDbClient.UpsertAsync    │
                         │  ↓                                   │
                         │  QDRANT  ← stored permanently        │
                         └──────────────────────────────────────┘

                         ┌──────────────────────────────────────┐
                         │  PHASES 1–5  (per user request)      │
                         │                                      │
  Browser form           │  ExpertController (MVC)              │
  ──────────────────────►│  or ExpertSystemChatController (API) │
                         │  ↓ UserProfileDto                    │
                         │  ExpertSystemService                 │
                         │  ↓ UserProfileMapper.ToDomain        │
                         │  ExpertSystemEngine (NRules)         │
                         │  ↓ fires 40+ rules                   │
                         │  Recommendation                      │
                         │  { Prefer, Avoid, Sillage,           │
                         │    Longevity, Reasons }              │
                         │  ↓                                   │
                         │  HybridExpertSystemService           │
                         │  ├─ embed Prefer terms               │
                         │  │  (OllamaEmbeddingClient)          │
                         │  ├─ QdrantVectorSearchClient         │
                         │  │  SearchAsync(vector, filter)      │
                         │  │  → top-5 RagRetrievedChunk[]      │
                         │  ├─ extract product DTOs + images    │
                         │  └─ IRagLLMClient.ExecuteAsync       │
                         │     → LLM-generated text             │
                         │  ↓ HybridEvaluationResult            │
                         │  { LlmResponse, Products[] }         │
                         │  ↓                                   │
                         │  HybridRecommendationViewModel       │
                         │  → View (product cards + LLM text)   │
                         └──────────────────────────────────────┘
```

---

## 2. Phase 0 — RAG Indexing Pipeline

**Entry point:** `RagIndexingHostedService` — a .NET `BackgroundService` that runs once when the app starts (controlled by `OnStartIndexing` env flag). It waits a few seconds (`DelaySeconds`) for Qdrant to be ready before beginning.

**Why index at all?**  
The LLM has no knowledge of your specific perfume catalogue. By converting every perfume into a vector and storing it in Qdrant, we can later find the most semantically relevant perfumes for any query — without the LLM needing to see every product.

---

### Step 1 — Extract from SQL (`ProductDataExtractor`)

```
ALOudDbContext.Perfumes
  .Include(Brand, Families, Notes, Accords, Tags, Seasons, Occasions)
  → PerfumeRagSource {
      Id, Name, Brand, Intensity, Longevity, Sillage,
      GenderProfile, PriceRange, Price, Description, ImageUrl,
      Families[], Notes[], Accords[], Tags[], Seasons[], Occasions[]
    }
```

**Why a dedicated extractor?** It cleanly separates the SQL concern (EF Core, lazy loading, N+1 prevention) from the text-building concern. The `.AsNoTracking()` call ensures EF doesn't cache these objects in memory during what could be a large batch.

---

### Step 2 — Build a text document (`DocumentBuilderService`)

Each `PerfumeRagSource` is serialised into a structured plain-text document:

```
Perfume Name: Aventus
Brand: Creed
Gender Profile: Masculine
Price: 1800 MAD

Description:
A timeless chypre-fruity fragrance inspired by Napoleon Bonaparte...

Performance Characteristics:
- Intensity: Moderate
- Longevity: Long
- Sillage: Moderate

Olfactory Families:
Chypre, Fruity

Notes:
- Blackcurrant (Top, level: top)
- Pineapple (Top, level: top)
- Birch (Base, level: base)
- Ambergris (Base, level: base)

Main Accords:
- Fruity (intensity: strong)
- Woody (intensity: medium)

Best Seasons:
Spring, Fall

Best Occasions:
Office, Formal

Tags:
masculine, designer, classic
```

**Why plain text and not JSON?** Embedding models are trained on natural language. A sentence like "Olfactory Families: Chypre, Fruity" is closer to the training distribution than `{"families":["Chypre","Fruity"]}`, so the resulting vector is more semantically meaningful.

---

### Step 3 — Split into chunks (`ChunkingService`)

| Parameter | Value |
|---|---|
| Max chunk size | 900 characters |
| Overlap | 120 characters |
| Split boundary | `\n\n` (paragraph break) |

The chunker fills a buffer paragraph by paragraph. When adding the next paragraph would exceed 900 chars, it flushes the buffer as a chunk, then starts the next chunk with the last 120 characters of the previous one (the overlap).

**Why chunk at all?** Embedding models have a token limit. A long perfume description might exceed it, and even if it doesn't, a single vector for a 2000-character document loses fine-grained meaning. Smaller chunks produce more precise vectors.

**Why overlap?** A sentence that straddles a chunk boundary should appear in both chunks, so neither loses half its context. 120 characters (~2 short sentences) is enough to preserve continuity without duplicating too much data.

Each chunk carries metadata forwarded from the source:

```
SourceId    → perfume GUID (for deduplication at query time)
ChunkIndex  → 0, 1, 2… (distinguishes chunks of the same perfume)
Brand, GenderProfile, PriceRange
Sillage, Longevity    ← used for exact-match filtering at query time
ImageUrl              ← surfaced in the recommendation view
```

---

### Step 4 — Generate embeddings (`OllamaEmbeddingClient`)

```
POST http://localhost:11434/api/embeddings
Body: { "model": "nomic-embed-text", "prompt": "<chunk text>" }
Response: { "embedding": [0.023, -0.14, …] }  // float[768]
```

**What is an embedding?**  
An embedding is a list of 768 floating-point numbers that encode the *semantic meaning* of the text. The model was trained so that texts with similar meanings land close together in this 768-dimensional space (measured by cosine similarity), regardless of exact wording. "Citrus woody scent for summer" and "fresh bergamot oak fragrance warm season" end up close together.

**Why `nomic-embed-text`?** It is a free, locally-run model with strong semantic quality at 768 dimensions. Running it locally (Ollama) avoids API costs and latency for batch indexing.

---

### Step 5 — Store in Qdrant (`QdrantVectorDbClient.UpsertAsync`)

```
PUT /collections/perfumes/points
Body: {
  "points": [
    {
      "id": "<deterministic-uuid>",
      "vector": [0.023, -0.14, …],
      "payload": {
        "content": "<chunk text>",
        "sourceId": "<perfume-guid>",
        "chunkIndex": 0,
        "brand": "Creed",
        "genderProfile": "Masculine",
        "priceRange": "Luxury",
        "sillage": "Moderate",
        "longevity": "Long",
        "imageUrl": "/images/aventus.jpg"
      }
    }
  ]
}
```

**Why a deterministic UUID as point ID?**  
Qdrant requires every point to have either a UUID or an unsigned integer as its ID. We need the ID to be stable across re-indexes (so upsert overwrites rather than duplicates). We can't use the perfume GUID directly because one perfume produces several chunks and Qdrant IDs must be unique. The solution is to derive a UUID per chunk by hashing `"<perfumeGuid>:<chunkIndex>"` with MD5 (16 bytes → valid Guid):

```csharp
// EmbeddingIndexService.DeriveChunkId
var input = Encoding.UTF8.GetBytes($"{sourceId}:{chunkIndex}");
var hash  = MD5.HashData(input);
return new Guid(hash);
```

This is deterministic (same input → same UUID every time), unique per (perfume, chunk) pair, and valid for Qdrant.

**Why `PUT` (upsert) instead of `POST` (insert)?** Re-indexing is safe: if a perfume already exists in Qdrant with that ID, the upsert replaces it. The hosted service also calls `DeleteBySourceIdAsync` before indexing each perfume to clean up any old chunks whose count may have changed.

---

## 3. Phase 1 — User Form → UserProfile

### Entry points

| URL | Controller | Input |
|---|---|---|
| `GET /Expert/HybridRecommendation` | `ExpertController` | — renders the form |
| `POST /Expert/HybridRecommendation` | `ExpertController` | `[FromForm] string recommendationJson` |
| `POST /api/v1/ai/expert-system/test` | `ExpertSystemChatController` | `[FromBody] UserProfileDto` |
| `POST /api/v1/ai/expert-system/evaluate` | `ExpertSystemChatController` | `[FromBody] RecommendationDto` |

### MVC path (main UI)

The Razor view serialises the user's choices to JSON and POSTs them as a hidden form field. The controller deserialises that JSON into `RecommendationDto` (pre-built preferences) and maps it directly to a `Recommendation` domain object, then calls the hybrid service.

### API `/test` path

The `/test` endpoint receives a `UserProfileDto` — the full 8-field profile — runs the NRules engine to derive preferences, then calls the hybrid service. This is the path that goes through the full expert system rule evaluation.

```csharp
public class UserProfileDto {
    public EClimate          Climate          // Hot | Cold | Humid | Mixed
    public EOccasion         Occasion         // Office | Formal | Date | Nightlife | Sport | Daily | Gym
    public ESkinType         SkinType         // Dry | Oily | Normal
    public EComplimentDesire Compliment       // Yes | No | Neutral
    public ESeasonPreference SeasonPreference // Spring | Summer | Fall | Winter | AllYear
    public EPersona          Persona          // Corporate | Sexy | Sporty | Artistic | Minimalist
                                              // Rebellious | Elegant | Youthful | Mature | Mysterious
    public ESensitivity      Sensitivity      // None | Migraine | HatesSweet | HatesSpice
                                              // HatesFloral | HatesFresh | PrefersMinimal
    public bool              WantsLongPerformance
}
```

`UserProfileMapper.ToDomain(dto)` is a 1-to-1 copy from DTO to domain object. It exists so the domain layer (`UserProfile`) never depends on the API layer (`UserProfileDto`).

---

## 4. Phase 2 — NRules Engine → Recommendation

### What is NRules?

NRules is a .NET rule engine based on the **Rete algorithm**. The Rete algorithm builds a network of condition nodes from all rules at startup. When facts are inserted into a session, they flow through this network — only rules whose conditions are fully satisfied enter the **agenda** (the list of rules ready to fire). This makes evaluation O(facts × rules) in the worst case, but in practice much faster because most rules are eliminated early in the network.

### How the engine runs

```csharp
// ExpertSystemEngine.Run(UserProfile profile)
var session = _factory.CreateSession();
session.Insert(profile);   // fact 1: the user's choices
session.Insert(rec);       // fact 2: the mutable Recommendation to populate
session.Fire();            // run all matching rules
return rec;
```

`_factory` is compiled once at startup (`Singleton`) by scanning all classes in the assembly that inherit from `Rule`. Each `Rule` subclass defines:
- `When()` — conditions on the facts in working memory
- `Then()` — actions to take (mutating `rec`)

Both `UserProfile` and `Recommendation` are in working memory simultaneously. Rules match on `UserProfile` fields and write into `Recommendation`. Because `Recommendation` starts empty, multiple rules can safely add to the same `HashSet<string>` (union semantics — no duplicates, order doesn't matter).

### Rule categories

**Climate rules** — respond to `EClimate`:

| Rule | Condition | Effect |
|---|---|---|
| `HotClimatePreferRule` | Climate = Hot | Prefer: `citrus, aquatic, green, light_musk` |
| `HotClimateAvoidRule` | Climate = Hot | Avoid: `heavy, oriental, animalic` |
| `HotClimateSillageRule` | Climate = Hot | Sillage = `moderate_or_intimate` *(if not already set)* |
| `HotClimateLongevityRule` | Climate = Hot | Longevity = `>= medium` *(if not already set)* |
| `ColdClimatePreferRule` | Climate = Cold | Prefer: `oud, amber, warm_spice, leather` |
| `ColdClimateSillageRule` | Climate = Cold | Sillage = `!= intimate` *(if not already set)* |
| `ColdClimateLongevityRule` | Climate = Cold | Longevity = `>= long` *(if not already set)* |
| `HumidClimateRule` | Climate = Humid | Prefer: `aromatic, woody, iso_e_super` |
| `MixedClimateRule` | Climate = Mixed | Prefer balanced notes |
| `DrySkinRule` | SkinType = Dry | Prefer: `gourmand, vanilla, musky` |
| `OilySkinRule` | SkinType = Oily | Avoid: `heavy_base, rich_resinous` |

**Occasion rules** — respond to `EOccasion`:

| Rule | Condition | Effect |
|---|---|---|
| `OfficePreferRule` | Occasion = Office | Prefer: `woody, musk, aromatic, clean` |
| `OfficeSillageRule` | Occasion = Office | Sillage = `light` *(unconditional — overrides climate)* |
| `OfficeLongevityRule` | Occasion = Office | Longevity = `medium` |
| `OfficeSweetnessRule` | Occasion = Office | Avoid: `gourmand` |
| `NightlifePreferRule` | Occasion = Nightlife | Prefer: `oud, amber, spicy, animalic` |
| `NightlifeSillageRule` | Occasion = Nightlife | Sillage = `heavy` |
| `NightlifeLongevityRule` | Occasion = Nightlife | Longevity = `>= long` |
| `DatePreferRule` | Occasion = Date | Prefer: romantic, sensual notes |
| `GymPreferRule` | Occasion = Gym | Prefer: `citrus, sport, aquatic` |
| `SportPreferRule` | Occasion = Sport | Prefer: `fresh, aquatic, green` |

**Performance rules** — respond to explicit performance preferences:

| Rule | Condition | Effect |
|---|---|---|
| `WantsLongPerformanceRule` | WantsLongPerformance = true | Longevity = `>= medium` |
| `MuskAmberVanillaBaseRule` | (profile) | Prefer long-lasting base notes |
| `HotClimateTopNotesRule` | Climate = Hot | Prefer volatile top notes |

**Persona rules** — respond to `EPersona`:

| Rule | Condition | Effect |
|---|---|---|
| `CorporatePersonaRule` | Persona = Corporate | Prefer: `woody, aromatic, musk` |
| `SexyPersonaRule` | Persona = Sexy | Prefer: `oud, amber, animalic` |
| `ArtisticPersonaRule` | Persona = Artistic | Prefer niche/unusual notes |
| `MinimalistPersonaRule` | Persona = Minimalist | Prefer: `clean, white_musk` |
| `ElegantPersonaRule` | Persona = Elegant | Prefer: `floral, powdery, iris` |
| `MysteriousPersonaRule` | Persona = Mysterious | Prefer: `smoky, incense, dark_amber` |

**Sensitivity rules** — respond to `ESensitivity`:

| Rule | Condition | Effect |
|---|---|---|
| `MigraineSensitivityRule` | Sensitivity = Migraine | Avoid: `oud, amber_heavy, animalic, incense_heavy` |
| `HatesSweetSensitivityRule` | Sensitivity = HatesSweet | Avoid: `gourmand, vanilla, caramel` |
| `HatesFloralSensitivityRule` | Sensitivity = HatesFloral | Avoid: `floral, rose, jasmine` |
| `HatesSpiceSensitivityRule` | Sensitivity = HatesSpice | Avoid: `spice, pepper, clove` |
| `HatesFreshSensitivityRule` | Sensitivity = HatesFresh | Avoid: `citrus, aquatic, green` |
| `PrefersMinimalSensitivityRule` | Sensitivity = PrefersMinimal | Prefer: `clean, fresh` |

**Compliment rules** — respond to `EComplimentDesire`:

| Rule | Condition | Effect |
|---|---|---|
| `ComplimentWorkRule` | Compliment = Yes + Occasion = Office | Keep sillage moderate (professional) |
| `ComplimentDateRule` | Compliment = Yes + Occasion = Date | Sensual notes, closer sillage |
| `ComplimentNightlifeRule` | Compliment = Yes + Occasion = Nightlife | Bold, projecting sillage |

### Sillage and Longevity: occasion wins over climate

`Sillage` and `Longevity` are scalar strings (last write wins). Climate rules could silently override occasion rules or vice versa depending on NRules' internal agenda order, which is non-deterministic across versions.

**Resolution:** Climate-level rules guard with a null-check and only write if no higher-priority rule has already set the value:

```csharp
// HotClimateSillageRule — climate level, defers to occasion rules
private static void Apply(Recommendation rec)
{
    if (string.IsNullOrWhiteSpace(rec.Sillage))
        rec.Sillage = "moderate_or_intimate";
    rec.Reasons.Add("Hot climate → moderate or intimate sillage recommended");
}

// OfficeSillageRule — occasion level, always writes
private static void Apply(Recommendation rec)
{
    rec.Sillage = "light";
    rec.Reasons.Add("Office occasion → light sillage required");
}
```

Because NRules fires all matching rules and occasions rules write unconditionally, the occasion value always ends up in `rec.Sillage` regardless of order.

### Output: the Recommendation object

```
Recommendation {
    Prefer:   HashSet<string>   // union of all prefer-sets from fired rules
    Avoid:    HashSet<string>   // union of all avoid-sets from fired rules
    Sillage:  string?           // last unconditional write (occasion > climate)
    Longevity:string?           // same
    Reasons:  List<string>      // one human-readable line per fired rule
}
```

**Example** for `Climate=Hot, Occasion=Office, Sensitivity=Migraine, WantsLongPerformance=true`:
```
Prefer:   { citrus, aquatic, green, light_musk, woody, musk, aromatic, clean }
Avoid:    { heavy, oriental, animalic, oud, amber_heavy, incense_heavy }
Sillage:  "light"       ← OfficeSillageRule (unconditional) wins
Longevity:"medium"      ← OfficeLongevityRule fires after WantsLongPerformance
Reasons:  [
  "Hot climate → prefer citrus, aquatic, green and light musk notes",
  "Hot climate → avoid heavy, oriental and animalic notes",
  "Hot climate → moderate or intimate sillage recommended",
  "Office occasion → woody, musk, aromatic and clean notes preferred",
  "Office → sillage should be light",
  "Migraine sensitivity → avoid oud, heavy amber, animalic and heavy incense",
  "User wants long performance → longevity should be at least medium"
]
```

---

## 5. Phase 3 — Vector Search → Top-5 Perfumes

**Service:** `HybridExpertSystemService.GetTopKProducts`

This phase answers the question: *which perfumes in our catalogue best match the user's preference profile?*

### Why vector search instead of SQL filters?

SQL filters on categories like `Family = 'Chypre'` are exact-match. The rules engine outputs terms like `"citrus"`, `"woody"`, `"aromatic"` — these are semantic concepts, not database column values. A perfume that is "fresh bergamot woody" should match the query `"citrus woody"` even if the word "citrus" never appears in its record. Vector search solves this because semantically similar texts produce similar vectors.

### Step 1 — Build the query text

```csharp
// Join all Prefer terms plus sillage/longevity hints into one query string
var queryTerms = prefer.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
if (!string.IsNullOrWhiteSpace(sillage))  queryTerms.Add(sillage);
if (!string.IsNullOrWhiteSpace(longevity)) queryTerms.Add(longevity);

var queryText = queryTerms.Count > 0
    ? string.Join(" ", queryTerms)    // e.g. "citrus aquatic woody clean light"
    : "perfume fragrance";
```

**Why include sillage and longevity in the query text?** These are performance characteristics that often appear in the embedded document text (e.g. "light sillage", "long-lasting"). Including them steers the embedding toward chunks that describe such characteristics.

### Step 2 — Embed the query

```csharp
var queryVector = await _embeddingClient.CreateEmbeddingAsync(queryText, cancellationToken);
// → float[768]
```

The same `OllamaEmbeddingClient` / `nomic-embed-text` model is used here as during indexing. **This is critical**: query and documents must be embedded with the same model, otherwise the cosine distances are meaningless.

### Step 3 — Build the metadata filter

`Prefer` terms are handled semantically by the query vector. `Sillage` and `Longevity` are exact scalar values stored in Qdrant metadata, so they are applied as hard filters:

```csharp
QdrantFilter? filter = null;
if (!string.IsNullOrWhiteSpace(sillage) || !string.IsNullOrWhiteSpace(longevity))
{
    filter = new QdrantFilter();
    if (!string.IsNullOrWhiteSpace(sillage))
        filter.Must.Add(new FilterCondition { Key = "sillage", Value = sillage.Trim() });
    if (!string.IsNullOrWhiteSpace(longevity))
        filter.Must.Add(new FilterCondition { Key = "longevity", Value = longevity.Trim() });
}
```

**Why not filter on `Avoid` terms in Qdrant?** The `Avoid` terms are semantic concepts (e.g. `"oud"`, `"animalic"`), not exact metadata field values stored per-chunk. Filtering them in Qdrant would require storing every accord/note as a metadata array and using `match.any`, which adds indexing complexity. Instead, `Avoid` terms are passed directly to the LLM system prompt, which has full understanding of what they mean and can exclude or warn about matching perfumes in natural language.

### Step 4 — Qdrant ANN vector search

```csharp
var results = await _vectorSearchClient.SearchAsync(queryVector, topK: 5, filter, cancellationToken);
```

**How does Qdrant find the nearest vectors?**

Qdrant uses **Hierarchical Navigable Small World (HNSW)** — an approximate nearest-neighbour (ANN) graph index. During indexing, each vector is connected to its nearest neighbours in a multi-layer graph. At search time, the algorithm starts at an entry point and greedily navigates to nodes closer to the query vector, exploring the neighbourhood at each layer. This finds the approximate top-K closest vectors in sub-linear time instead of scanning every point.

**What is cosine similarity?**  
Two vectors A and B have a cosine similarity of `cos(θ) = (A·B) / (|A|×|B|)`. A value of `1.0` means identical direction (same semantic meaning), `0.0` means orthogonal (unrelated). Qdrant uses this as the scoring function when the collection is configured with cosine distance.

The HTTP call Qdrant receives:

```json
POST /collections/perfumes/points/search
{
  "vector": [0.023, -0.14, …],
  "limit": 5,
  "with_payload": true,
  "filter": {
    "must": [
      { "key": "sillage",   "match": { "value": "light" } },
      { "key": "longevity", "match": { "value": "medium" } }
    ]
  }
}
```

Qdrant returns the 5 most similar chunks that also satisfy all `must` conditions, ordered by score descending.

### Step 5 — Deduplicate by source perfume

One perfume can produce several chunks (chunk 0 = name/brand/price, chunk 1 = notes, etc.). After the search, all chunks from the same perfume source are grouped and only the top-scoring chunk per perfume is kept:

```csharp
return results
    .GroupBy(chunk => chunk.Metadata["sourceId"].ToString())
    .Select(g => g.First())   // First() = highest score (results are score-ordered)
    .ToList();
```

**Why deduplicate?** Without this, a very descriptive perfume could fill all 5 slots with its own chunks, crowding out other candidates.

---

## 6. Phase 4 — LLM Generation → Final Answer

**Service:** `HybridExpertSystemService.GetLLMGeneratedRecommendationAsync`

### Why use an LLM here?

The vector search returns the right *candidates*, but a raw dump of chunk text is not a useful recommendation. The LLM synthesises the candidates, matches them against the user's reasons, respects the avoid constraints, and produces a concise, human-readable explanation.

### Prompt construction

```
SYSTEM:
  "You are a perfume recommendation expert. Recommend only from the provided
   products. Explain why each product fits the user profile.
   Characteristics to avoid: oud, amber_heavy, animalic."
                              ↑ Avoid terms from Recommendation.Avoid

USER:
  "Generate a concise recommendation with top products, why they fit,
   and a short caution for any trade-off."

CONTEXT (JSON):
  {
    "reasons": "- Hot climate → prefer citrus...\n- Office → sillage light...",
    "products": [
      {
        "id": "...",
        "sourceId": "<perfume-guid>",
        "name": "Aventus",
        "brand": "Creed",
        "content": "Perfume Name: Aventus\nBrand: Creed\n...",
        "metadata": { "brand": "Creed", "sillage": "Moderate", ... }
      },
      ...
    ]
  }
```

The `reasons` array (produced by the NRules engine) tells the LLM *why* each preference was derived, so it can ground its explanation in the user's actual input (skin type, climate, occasion) rather than making generic statements.

### LLM provider selection

The active provider is controlled by the `LLM_PROVIDER` environment variable (default: `"gemini"`):

| Value | Client | API |
|---|---|---|
| `"gemini"` | `GeminiLLMClient` | Google Gemini REST API |
| `"groq"` | `GroqLLMClient` | Groq OpenAI-compatible API |
| `"grok"` | `GrokLLMClient` | xAI Grok API |

All three inherit from `BaseLLMClient`, which handles:
- JSON serialisation of the provider-specific payload
- HTTP `POST` to the provider URL
- 429 (rate limit) and 403 (forbidden) error handling
- Response parsing into `RagLLMResult { FinalAnswer, ToolCall }`

The `LLMClientFactory` reads the env var at runtime and creates the correct instance.

### Fallback

If the LLM call fails (network error, rate limit, empty response), `BuildFallbackResponse` returns a plain text list:

```
Recommended products:
- Aventus by Creed
- Sauvage by Dior
```

This ensures the user always gets *something* even when the LLM is unavailable.

---

## 7. Phase 5 — Response to the View

### Service return type

`HybridExpertSystemService.EvaluateAsync` returns:

```csharp
public class HybridEvaluationResult {
    public string LlmResponse { get; init; }                 // LLM-generated text
    public IReadOnlyList<RecommendedPerfumeDto> Products { get; init; } // product cards
}

public class RecommendedPerfumeDto {
    public string  Name     { get; init; }
    public string  Brand    { get; init; }
    public string? ImageUrl { get; init; }  // from Qdrant metadata → Perfume.ImageUrl
}
```

`Name` and `Brand` are extracted from the chunk's `content` text using a line-prefix search (`"Perfume Name:"`, `"Brand:"`). `ImageUrl` comes directly from `chunk.Metadata["imageUrl"]`, which was stored during indexing.

### View model

```csharp
public class HybridRecommendationViewModel {
    public RecommendationDto? Recommendation { get; set; }      // user's input (for display)
    public string? LlmGeneratedResponse { get; set; }           // LLM text
    public IReadOnlyList<RecommendedPerfumeDto> Products { get; set; } // product cards
}
```

### What the view renders

`Views/Expert/HybridRecommendation.cshtml` renders three cards in order:

1. **Preference Profile** — the `Prefer` tags (green), `Avoid` tags (red), sillage and longevity values.
2. **Recommended Perfumes** — a responsive CSS grid of product cards. Each card shows:
   - The perfume image (`<img src="@product.ImageUrl">`) if available
   - A `bi-droplet` placeholder icon if `ImageUrl` is null
   - The perfume name and brand underneath
3. **AI Analysis** — the raw LLM-generated recommendation text, followed by the NRules `Reasons` list that explains *why* those preferences were derived.

> **Note on images:** Images appear after the next re-index run. Until Qdrant is repopulated with the updated metadata (which now includes `imageUrl`), existing chunks return `null` for `ImageUrl` and the placeholder is shown.
