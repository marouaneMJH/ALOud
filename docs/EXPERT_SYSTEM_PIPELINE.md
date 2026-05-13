# Expert System — Recommendation Pipeline

This document describes the complete flow from the user filling the form to receiving a perfume recommendation. It covers every service, every transformation, all known bugs, and the best-available fix for each.

---

## Table of Contents

1. [Big Picture](#1-big-picture)
2. [Phase 0 — RAG Indexing (background, runs once)](#2-phase-0--rag-indexing-background-runs-once)
3. [Phase 1 — User Form → UserProfileDto](#3-phase-1--user-form--userprofiledto)
4. [Phase 2 — NRules Engine → Recommendation](#4-phase-2--nrules-engine--recommendation)
5. [Phase 3 — Qdrant Filter Search → RagRetrievedChunk\[\]](#5-phase-3--qdrant-filter-search--ragretrievedchunk)
6. [Phase 4 — LLM Generation → Final Answer](#6-phase-4--llm-generation--final-answer)
7. [Known Bugs & Fixes](#7-known-bugs--fixes)

---

## 1. Big Picture

```
┌─────────────────────────────────────────────────────────────────────────┐
│  BACKGROUND (once on startup)                                           │
│                                                                         │
│  SQL Server ──► ProductDataExtractor ──► DocumentBuilderService         │
│                      │                         │                        │
│                      ▼                         ▼                        │
│              PerfumeRagSource          plain-text document              │
│                                                │                        │
│                                       ChunkingService                   │
│                                                │                        │
│                                       RagDocumentChunk[]                │
│                                                │                        │
│                                    OllamaEmbeddingClient                │
│                                    (nomic-embed-text, 768d)             │
│                                                │                        │
│                                       VectorRecord[]                    │
│                                                │                        │
│                                    QdrantVectorDbClient.UpsertAsync     │
│                                                │                        │
│                                           QDRANT DB                     │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│  REQUEST PATH (per user request)                                        │
│                                                                         │
│  HybridRecommendation.cshtml (form)                                     │
│      │  POST recommendationJson                                         │
│      ▼                                                                  │
│  ExpertController.HybridRecommendation (MVC)                            │
│    OR                                                                   │
│  ExpertSystemChatController.Test (API)                                  │
│      │                                                                  │
│      ▼  UserProfileDto                                                  │
│  ExpertSystemService.Evaluate                                           │
│      │  UserProfileMapper.ToDomain                                      │
│      │  ExpertSystemEngine.Run  ◄── NRules (40+ rules)                  │
│      │                                                                  │
│      ▼  Recommendation { Prefer, Avoid, Sillage, Longevity, Reasons }  │
│                                                                         │
│  HybridExpertSystemService.EvaluateAsync                                │
│      │                                                                  │
│      ├─ BuildQdrantFilter(Recommendation)                               │
│      │     └─ QdrantFilter { Must[], MustNot[] }          ⚠ BUG #1     │
│      │                                                                  │
│      ├─ QdrantPayloadSearchClient.SearchByFilterAsync     ⚠ BUG #2     │
│      │     └─ Qdrant /points/scroll (no vector scoring)                 │
│      │                                                                  │
│      ├─ Deduplicate by sourceId → top 5 RagRetrievedChunk[]             │
│      │                                                                  │
│      └─ IRagLLMClient.ExecuteAsync (Gemini / Groq / Grok)               │
│            └─ final answer string                                       │
│                                                                         │
│  HybridRecommendationViewModel → View                                   │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Phase 0 — RAG Indexing (background, runs once)

**Entry point:** `RagIndexingHostedService` — a `BackgroundService` that fires on startup when `OnStartIndexing = true`.

### Steps

#### 2.1 Extract from SQL — `ProductDataExtractor`
```
ALOudDbContext.Perfumes
  .Include(Brand, Families, Notes, Accords, Tags, Seasons, Occasions)
  → PerfumeRagSource { Id, Name, Brand, Intensity, Longevity, Sillage,
                       GenderProfile, PriceRange, Price, Description,
                       Families[], Notes[], Accords[], Tags[],
                       Seasons[], Occasions[] }
```

#### 2.2 Build document — `DocumentBuilderService`
Converts one `PerfumeRagSource` into a multi-line plain-text document:
```
Perfume Name: Aventus
Brand: Creed
Gender Profile: Masculine
Price: 1800 MAD

Description:
A timeless chypre-fruity fragrance...

Performance Characteristics:
- Intensity: Moderate
- Longevity: Long
- Sillage: Moderate

Olfactory Families:
Chypre, Fruity

Notes:
- Blackcurrant (Top, level: top)
- Birch (Base, level: base)
...

Main Accords:
- Fruity (intensity: strong)
...

Best Seasons:
Spring, Fall

Best Occasions:
Office, Formal

Tags:
masculine, designer
```
This text is what gets embedded and stored in Qdrant. It is the only content available for retrieval.

#### 2.3 Chunk — `ChunkingService`
- Max chunk size: **900 chars**
- Overlap: **120 chars** (last 120 chars of previous chunk prepended to next)
- Split boundary: `\n\n` (paragraph break)
- Each chunk carries: `SourceId` (perfume Guid), `ChunkIndex`, `Brand`, `GenderProfile`, `PriceRange`

#### 2.4 Embed — `OllamaEmbeddingClient`
Calls `POST http://localhost:11434/api/embeddings` with model `nomic-embed-text`.  
Returns a `float[768]` vector per chunk.

#### 2.5 Store — `QdrantVectorDbClient.UpsertAsync`
Each `VectorRecord` is upserted into Qdrant collection `perfumes` with:
- `id`: `chunk.SourceId.ToString()` ← **⚠ Bug #3** (overrides sibling chunks)
- `vector`: `float[768]`
- `payload.content`: the chunk text
- `payload.sourceId`, `payload.chunkIndex`, `payload.brand`, `payload.genderProfile`, `payload.priceRange`

> **Missing payload fields:** `characteristics`, `sillage`, `longevity` are **never stored** in Qdrant — ← **⚠ Bug #4** (the filter phase relies on these fields)

---

## 3. Phase 1 — User Form → UserProfileDto

### Entry points

| Path | Controller |
|---|---|
| `GET /Expert/HybridRecommendation` | `ExpertController.HybridRecommendation()` → renders the form |
| `POST /Expert/HybridRecommendation` | `ExpertController.HybridRecommendation([FromForm] string recommendationJson)` |
| `POST /api/v1/ai/expert-system/test` | `ExpertSystemChatController.Test([FromBody] UserProfileDto)` |
| `POST /api/v1/ai/expert-system/evaluate` | `ExpertSystemChatController.Evaluate([FromBody] RecommendationDto)` |

### MVC flow (main UI path)

The Razor view `Views/Expert/HybridRecommendation.cshtml` renders a form.  
On submit it serializes user choices to JSON and POSTs the string in a hidden field `recommendationJson`.

The controller deserializes it into `RecommendationDto` (the DTO in `DTOs/ExpertSystem/RecommendationDto.cs`):

```csharp
public class RecommendationDto {
    public List<string>? Prefer { get; set; }   // e.g. ["citrus","woody"]
    public List<string>? Avoid  { get; set; }   // e.g. ["oud"]
    public string?       Sillage    { get; set; }
    public string?       Longevity  { get; set; }
    public List<string>? Reasons    { get; set; }
    public string?       Result     { get; set; }  // filled on response
}
```

### API flow (Test endpoint)

The API `/test` endpoint receives `UserProfileDto` — the full profile form:

```csharp
public class UserProfileDto {
    public EClimate          Climate          { get; set; }  // Hot|Cold|Humid|Mixed
    public EOccasion         Occasion         { get; set; }  // Office|Formal|Date|Nightlife|Sport|Daily|Gym
    public ESkinType         SkinType         { get; set; }  // Dry|Oily|Normal
    public EComplimentDesire Compliment       { get; set; }  // Yes|No|Neutral
    public ESeasonPreference SeasonPreference { get; set; }  // Spring|Summer|Fall|Winter|AllYear
    public EPersona          Persona          { get; set; }  // Corporate|Sexy|Sporty|Artistic|...
    public ESensitivity      Sensitivity      { get; set; }  // None|Migraine|HatesSweet|...
    public bool              WantsLongPerformance { get; set; }
}
```

`UserProfileMapper.ToDomain(dto)` copies every field 1-to-1 into `UserProfile` (domain object).

---

## 4. Phase 2 — NRules Engine → Recommendation

**Services:** `ExpertSystemService` → `ExpertSystemEngine` → NRules

### How NRules works here

```csharp
// ExpertSystemEngine.Run(UserProfile profile)
var session = _factory.CreateSession();
session.Insert(profile);     // working memory: the user's profile
session.Insert(rec);         // working memory: the mutable Recommendation object
session.Fire();              // match & execute all matching rules
return rec;                  // Recommendation is now populated
```

NRules uses a **Rete network** — each rule defines `When()` conditions and `Then()` actions.  
Rules mutate the shared `Recommendation` object by calling `rec.Prefer.UnionWith(...)`, `rec.Avoid.UnionWith(...)`, setting `rec.Sillage`, `rec.Longevity`, and appending to `rec.Reasons`.

### Rule categories (40+ rules total)

| Category | Example rule | What it adds |
|---|---|---|
| **ClimateRules** | `HotClimatePreferRule` | Prefer: `citrus, aquatic, green, light_musk` |
| | `ColdClimatePreferRule` | Prefer: `oud, amber, warm_spice, leather` |
| | `HumidClimateRule` | Prefer: `aromatic, woody, iso_e_super` |
| | `HotClimateAvoidRule` | Avoid: `heavy, oriental, animalic` |
| | `HotClimateSillageRule` | Sillage: `moderate` |
| | `ColdClimateSillageRule` | Sillage: `heavy` |
| | `HotClimateLongevityRule` | Longevity: `>= medium` |
| | `ColdClimateLongevityRule` | Longevity: `>= long` |
| | `DrySkinRule` | Prefer: `gourmand, vanilla, musky` |
| | `OilySkinRule` | Avoid: `heavy_base, rich_resinous` |
| **OccasionRules** | `OfficePreferRule` | Prefer: `woody, musk, aromatic, clean` |
| | `OfficeSillageRule` | Sillage: `light` |
| | `NightlifePreferRule` | Prefer: `oud, amber, spicy, animalic` |
| | `NightlifeSillageRule` | Sillage: `heavy` |
| | `DatePreferRule` | Prefer romantic notes |
| | `GymPreferRule` | Prefer: `citrus, sport, aquatic` |
| **PerformanceRules** | `WantsLongPerformanceRule` | Longevity: `>= medium` |
| | `NightlifeLongevityRule` | Longevity: `>= long` |
| | `MuskAmberVanillaBaseRule` | Prefer base notes for projection |
| | `HotClimateTopNotesRule` | Prefer volatile top notes |
| **PersonaRules** | `CorporatePersonaRule` | Prefer: `woody, aromatic, musk` |
| | `SexyPersonaRule` | Prefer: `oud, amber, animalic` |
| | `ArtisticPersonaRule` | Prefer niche/unusual notes |
| | `MinimalistPersonaRule` | Prefer: `clean, white_musk` |
| **ComplimentRules** | `ComplimentWorkRule` | Moderate sillage for office |
| | `ComplimentDateRule` | Sensual notes for date context |
| | `ComplimentNightlifeRule` | Bold sillage for nightlife |
| **SensitivityRules** | `MigraineSensitivityRule` | Avoid: `oud, amber_heavy, animalic, incense_heavy` |
| | `HatesSweetSensitivityRule` | Avoid: `gourmand, vanilla, caramel` |
| | `HatesFloralSensitivityRule` | Avoid: `floral, rose, jasmine` |
| | `PrefersMinimalSensitivityRule` | Prefer: `clean, fresh` |

### Recommendation output

```csharp
public class Recommendation {
    public HashSet<string> Prefer  { get; }  // union of all rule outputs
    public HashSet<string> Avoid   { get; }  // union of all rule outputs
    public string?         Sillage   { get; set; }  // last rule wins
    public string?         Longevity { get; set; }  // last rule wins
    public List<string>    Reasons { get; }  // one line per fired rule
}
```

**Example output** for `Climate=Hot, Occasion=Office, Sensitivity=Migraine, WantsLongPerformance=true`:
```
Prefer:  { citrus, aquatic, green, light_musk, woody, musk, aromatic, clean }
Avoid:   { oud, amber_heavy, animalic, incense_heavy, heavy, oriental }
Sillage: "light"       (OfficeSillageRule wins over HotClimateSillageRule)
Longevity: ">= medium" (WantsLongPerformanceRule)
Reasons: [
  "Hot climate → prefer citrus, aquatic, green and light musk notes",
  "Office occasion → woody, musk, aromatic and clean notes preferred",
  "Office → sillage should be light",
  "Migraine sensitivity → avoid oud, heavy amber, animalic and heavy incense",
  "User wants long performance → longevity should be at least medium"
]
```

> **Note on `Sillage` / `Longevity` conflicts:** Multiple rules can write the same field.  
> The last rule to fire wins (NRules fires rules in an unspecified order unless priority is set).  
> This can cause silent overrides — for example `WantsLongPerformanceRule` sets `>= medium`  
> but `NightlifeLongevityRule` also sets `>= long`. No conflict resolution logic exists.

---

## 5. Phase 3 — Qdrant Filter Search → RagRetrievedChunk[]

**Service:** `HybridExpertSystemService.GetTopKProducts`

### What should happen (design intent)

```
Recommendation.Prefer  → QdrantFilter.Must   (key = "characteristics")
Recommendation.Avoid   → QdrantFilter.MustNot (key = "characteristics")
Recommendation.Sillage → QdrantFilter.Must   (key = "sillage")
Recommendation.Longevity → QdrantFilter.Must (key = "longevity")

QdrantPayloadSearchClient.SearchByFilterAsync(topK=5, filter)
  → POST /collections/perfumes/points/scroll
  → returns up to 5 RagRetrievedChunk[]
  → deduplicated by sourceId
```

### What actually happens (broken)

```csharp
// HybridExpertSystemService.cs line 74-77
var results = await _payloadSearchClient.SearchByFilterAsync(
    topK,
    filter: null,      // ← BUG #1: filter is built but never passed
    cancellationToken: cancellationToken);
```

Because `filter: null` is hardcoded, **every request returns the same top-5 results from Qdrant** regardless of user preferences.

Even if the filter were passed, the Qdrant payloads don't contain a `characteristics` field (BUG #4), so `Must` conditions on `"characteristics"` would match nothing.

### How `QdrantPayloadSearchClient` works

It calls the Qdrant **scroll** endpoint (`/points/scroll`), not the **search** endpoint (`/points/search`).

| | `scroll` | `search` |
|---|---|---|
| Ranking | none (arbitrary order) | cosine similarity score |
| Requires vector | no | yes |
| Use case | paginate / filter | nearest-neighbour |

The scroll endpoint is correct for **pure filter-based lookup** but wrong for **semantic ranking**. The ideal approach is a **hybrid search**: embed the user's preference terms as a query vector, then apply Qdrant's filtered vector search.

---

## 6. Phase 4 — LLM Generation → Final Answer

**Service:** `HybridExpertSystemService.GetLLMGeneratedRecommendationAsync`

### Prompt construction

```
System:
  "You are a perfume recommendation expert. Recommend only from provided products.
   Respect avoid constraints and explain why each product fits the user profile."

User:
  "Generate a concise recommendation response with top products, why they fit,
   and a short caution for any trade-off."

Context (JSON serialized):
  {
    "reasons": "- Hot climate → prefer citrus...\n- Office → sillage light...",
    "products": [
      { "id": "...", "sourceId": "...", "name": "Aventus", "brand": "Creed",
        "content": "Perfume Name: Aventus\nBrand: Creed\n...",
        "metadata": { "brand": "Creed", "genderProfile": "Masculine", ... }
      },
      ...
    ]
  }
```

### LLM client selection

`LLMClientFactory.CreateClient()` reads `LLM_PROVIDER` env var (default: `"gemini"`):
- `"gemini"` → `GeminiLLMClient` (calls Gemini REST API)
- `"groq"` → `GroqLLMClient` (calls Groq OpenAI-compatible API)
- `"grok"` → `GrokLLMClient` (calls xAI Grok API)

All three inherit from `BaseLLMClient`, which:
1. Serializes the payload
2. Sends `POST` to the provider URL
3. Handles 429 (rate limit) and 403 (forbidden) errors
4. Parses the response into `RagLLMResult { FinalAnswer, ToolCall }`

### Fallback

If the LLM fails or returns empty, `BuildFallbackResponse` returns a plain list:
```
Recommended products:
- Aventus by Creed
- Sauvage by Dior
...
```

### Final output

`HybridRecommendationViewModel.LlmGeneratedResponse` is set to the LLM-generated string.  
The Razor view renders it as the recommendation result.

---

## 7. Known Bugs & Fixes

### Bug #1 — Filter is built but never passed (CRITICAL)

**File:** `Services/Infrastructure/ExpertSystem/HybridExpertSystemService.cs:74`

```csharp
// CURRENT (broken)
var results = await _payloadSearchClient.SearchByFilterAsync(
    topK,
    filter: null,          // ← filter is discarded
    cancellationToken: cancellationToken);

// FIX
var results = await _payloadSearchClient.SearchByFilterAsync(
    topK,
    filter: filter,        // ← pass the built filter
    cancellationToken: cancellationToken);
```

**Impact:** Every user gets the same results. No preference-based filtering happens at all.

---

### Bug #2 — Qdrant uses `scroll` (unranked) instead of `search` (vector-ranked)

**File:** `Services/Infrastructure/Rag/Clients/QdrantPayloadSearchClient.cs:66`

The scroll endpoint returns arbitrary points. For a recommendation system, results should be ranked by relevance.

**Best fix:** Replace payload scroll with Qdrant's **filtered vector search**:

```csharp
// 1. Build a query string from the Prefer terms
var queryText = string.Join(", ", prefer.Union(new[] { sillage, longevity }
    .Where(x => !string.IsNullOrWhiteSpace(x))!));

// 2. Embed the query
var queryVector = await _embeddingClient.CreateEmbeddingAsync(queryText, cancellationToken);

// 3. Call vector search endpoint with filter
// POST /collections/perfumes/points/search
// { "vector": [...], "limit": 5, "with_payload": true,
//   "filter": { "must": [...], "must_not": [...] } }
```

**Simpler short-term fix** (keeps scroll but at least passes the filter):  
Apply Bug #1 fix + Bug #4 fix so the filter actually matches metadata fields.

---

### Bug #3 — All chunks of a perfume share the same Qdrant ID (data loss)

**File:** `Services/Infrastructure/Rag/IndexingJob/EmbeddingIndexService.cs:28`

```csharp
// CURRENT (broken) — SourceId is the perfume Guid, not chunk-unique
results.Add(new VectorRecord
{
    Id = chunk.SourceId.ToString(),   // ← all chunks overwrite each other
    ...
});

// FIX — use a deterministic per-chunk ID
results.Add(new VectorRecord
{
    Id = $"{chunk.SourceId}_{chunk.ChunkIndex}",
    ...
});
```

**Impact:** For any perfume with more than one chunk, only the last chunk survives in Qdrant.  
The first chunk (containing the name, brand, price) gets overwritten.

---

### Bug #4 — Filter keys `characteristics`, `sillage`, `longevity` are never stored in Qdrant

**File:** `Services/Infrastructure/Rag/IndexingJob/EmbeddingIndexService.cs:42-52`

The filter built in `BuildQdrantFilter` looks for Qdrant payload fields named `characteristics`, `sillage`, and `longevity`. These fields are never stored during indexing.

```csharp
// CURRENT metadata stored per chunk:
{ "sourceId", "chunkIndex", "brand", "genderProfile", "priceRange" }

// NEEDED to make filtering work:
{ "sourceId", "chunkIndex", "brand", "genderProfile", "priceRange",
  "sillage", "longevity", "accords" }
```

**Fix in `EmbeddingIndexService.BuildMetadata`:**

```csharp
private static Dictionary<string, object> BuildMetadata(RagDocumentChunk chunk)
{
    var meta = new Dictionary<string, object>
    {
        ["sourceId"]     = chunk.SourceId.ToString(),
        ["chunkIndex"]   = chunk.ChunkIndex,
        ["brand"]        = chunk.Brand        ?? string.Empty,
        ["genderProfile"]= chunk.GenderProfile ?? string.Empty,
        ["priceRange"]   = chunk.PriceRange   ?? string.Empty,
    };

    if (!string.IsNullOrWhiteSpace(chunk.Sillage))
        meta["sillage"] = chunk.Sillage;

    if (!string.IsNullOrWhiteSpace(chunk.Longevity))
        meta["longevity"] = chunk.Longevity;

    return meta;
}
```

This also requires passing `Sillage` and `Longevity` from `PerfumeRagSource` through `RagDocumentChunk` (add those fields to `RagDocumentChunk` and `ChunkingService.CreateChunk`).

> **Note on `characteristics`:** The Qdrant filter uses `"characteristics"` as a key for the `Prefer`/`Avoid` terms. These are olfactory note/accord names like `"citrus"`, `"woody"`. Storing all accords as a flat `characteristics` array in the payload would work, but requires Qdrant array matching syntax (`match.any`), not scalar `match.value`. The alternative — and arguably better solution — is **Bug #2's fix** (vector search), which finds semantically similar perfumes without needing exact keyword matches on payload fields.

---

### Bug #5 — Duplicate `RecommendationDto` class

Two definitions exist in the codebase:

| Location | Namespace |
|---|---|
| `DTOs/ExpertSystem/RecommendationDto.cs` | `ALOud.DTOs.ExpertSystem` |
| `Controllers/Api/v1/ExpertSystemChatController.cs` (bottom) | `ALOud.Controllers.Api.v1` |

The one inside the controller file is redundant. **Fix:** delete the inline class from the controller file and use only the one in `DTOs/ExpertSystem/`.

---

### Bug #6 — `HumidClimateRule.cs` has a leading space in its filename

**File:** `Services/Infrastructure/ExpertSystem/Rules/ HumidClimateRule.cs` (note the space)

NRules loads rules by scanning the assembly, not by filename, so this rule still fires correctly at runtime. However:
- The file cannot be opened by path on case-sensitive filesystems without quoting
- It breaks glob patterns and tooling

**Fix:** rename the file to `HumidClimateRule.cs`.

---

### Bug #7 — `Sillage`/`Longevity` rule conflicts (silent last-write-wins)

Multiple rules write `rec.Sillage` or `rec.Longevity` without checking the existing value. NRules fires rules in agenda order (non-deterministic unless priority is set).

**Example conflict:**
- `OfficeSillageRule` → `rec.Sillage = "light"`
- `HotClimateSillageRule` → `rec.Sillage = "moderate"`

Whichever fires last wins silently.

**Best fix:** Use a priority-based or accumulation-based approach:

```csharp
// Option A: NRules priority attribute (deterministic but fragile)
[Priority(10)]
public class OfficeSillageRule : Rule { ... }

// Option B: Store a list of sillage suggestions and resolve in service
rec.SillageSuggestions.Add("light");  // each rule adds, service picks
```

Option B is more robust because it makes the conflict visible in `Reasons`.

---

## Summary of Required Changes

| # | Severity | File | Fix |
|---|---|---|---|
| 1 | **Critical** | `HybridExpertSystemService.cs:74` | Pass `filter:filter` not `filter:null` |
| 2 | High | `QdrantPayloadSearchClient.cs` | Migrate to vector search endpoint |
| 3 | High | `EmbeddingIndexService.cs:28` | Use `$"{SourceId}_{ChunkIndex}"` as ID |
| 4 | High | `EmbeddingIndexService.cs:42` + `RagDocumentChunk` | Store `sillage`, `longevity` in metadata |
| 5 | Medium | `ExpertSystemChatController.cs` | Remove duplicate `RecommendationDto` class |
| 6 | Low | `Rules/ HumidClimateRule.cs` | Rename file (remove leading space) |
| 7 | Medium | All sillage/longevity rules | Add rule priorities or accumulation |
