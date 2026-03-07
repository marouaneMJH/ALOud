    # Expert System — ALOud

    > A rule-based perfume recommendation engine built with [NRules](https://nrules.net/) (a .NET forward-chaining inference engine).

    ---

    ## Overview

    The expert system takes a **user profile** (lifestyle, preferences, sensitivities) and produces a **recommendation** listing which olfactive notes/families to prefer or avoid, along with the reasoning behind each decision.

    It works entirely **in-memory and stateless**: no database query is involved — rules fire in a single session and the result is returned immediately.

    ---

    ## Architecture

    ```
    HTTP Request (JSON)
        │
        ▼
    ExpertSystemChatController   ← ASP.NET Core Web API
        │   POST /api/ai/expert-system/expert-test
        │   Body: UserProfileDto
        │
        ▼
    IExpertSystemService
        │
    ExpertSystemService          ← Application Service (Singleton)
        │  1. Maps DTO → Domain model via UserProfileMapper
        │  2. Delegates to the engine
        │
        ▼
    ExpertSystemEngine           ← NRules session orchestrator
        │  1. Creates an NRules session
        │  2. Inserts UserProfile + Recommendation into working memory
        │  3. Calls session.Fire() → all matching rules execute
        │
        ▼
    Rule Classes (50+ rules)     ← NRules Fluent DSL
        │  Each rule inspects the UserProfile facts
        │  and mutates the Recommendation object
        │
        ▼
    Recommendation               ← Aggregated output object
        (returned as JSON)
    ```

    ---

    ## Domain Models

    ### `UserProfile` — Input Facts

    | Property | Type | Description |
    |---|---|---|
    | `Climate` | `EClimate` | User's local climate (Hot, Cold, Humid, Mixed) |
    | `Occasion` | `EOccasion` | Primary wearing context (Office, Formal, Date, Nightlife, Sport, Daily, Gym) |
    | `SkinType` | `ESkinType` | Skin chemistry (Dry, Oily, Normal) |
    | `Compliment` | `EComplimentDesire` | Whether the user wants to attract compliments (Yes, No, Neutral) |
    | `SeasonPreference` | `ESeasonPreference` | Preferred season (Spring, Summer, Fall, Winter, AllYear) |
    | `Persona` | `EPersona` | Personality archetype (Corporate, Sexy, Sporty, Artistic, Minimalist, Rebellious, Elegant, Youthful, Mature, Mysterious) |
    | `Sensitivity` | `ESensitivity` | Olfactive sensitivities (None, Migraine, HatesSweet, HatesSpice, HatesFloral, HatesFresh, PrefersMinimal) |
    | `WantsLongPerformance` | `bool` | Whether the user explicitly wants a long-lasting fragrance |

    ### `Recommendation` — Output

    | Property | Type | Description |
    |---|---|---|
    | `Prefer` | `HashSet<string>` | Olfactive notes/families to include in search |
    | `Avoid` | `HashSet<string>` | Olfactive notes/families to exclude from search |
    | `Sillage` | `string?` | Recommended sillage level (e.g. `"light"`, `"moderate"`) |
    | `Longevity` | `string?` | Required longevity constraint (e.g. `">= medium"`) |
    | `Reasons` | `List<string>` | Human-readable explanation for every decision made |

    ---

    ## How the Engine Works (NRules)

    The engine uses **NRules**, a production rule engine for .NET based on the Rete algorithm.

    ### Lifecycle

    ```
    var repository = new RuleRepository();
    repository.Load(x => x.From(typeof(ExpertSystemEngine).Assembly));  // auto-discovers all Rule subclasses
    _factory = repository.Compile();                                      // compiled once at startup (Singleton)
    ```

    At request time:

    ```csharp
    var session = _factory.CreateSession();
    session.Insert(profile);       // assert UserProfile fact
    session.Insert(rec);           // assert mutable Recommendation fact
    session.Fire();                // run all matching rules
    return rec;                    // return enriched recommendation
    ```

    All rules that match the current facts fire **in a single pass**. Because each rule independently writes to `Prefer`, `Avoid`, `Sillage`, or `Longevity`, the results **accumulate additively** — multiple rules can all contribute to the final recommendation without conflict.

    ### Rule Pattern

    Every rule follows the same structure:

    ```csharp
    public class HotClimatePreferRule : Rule
    {
        public override void Define()
        {
            UserProfile user = null!;
            Recommendation rec = null!;

            When()
                .Match(() => user, u => u.Climate == EClimate.Hot)  // condition
                .Match(() => rec);                                   // bind output

            Then()
                .Do(_ => Apply(rec));                                // action
        }

        private static void Apply(Recommendation rec)
        {
            rec.Prefer.UnionWith(new[] { "citrus", "aquatic", "green", "light_musk" });
            rec.Reasons.Add("Hot climate → prefer citrus, aquatic, green and light musk notes");
        }
    }
    ```

    - **`When()`** — pattern matching conditions on facts in the working memory.
    - **`Then()`** — side-effects on the `Recommendation` object.
    - **`Reasons`** — every rule appends a plain-English explanation, making the system fully **explainable**.

    ---

    ## Rule Categories

    All rules live under `Services/Infrastructure/ExpertSystem/Rules/` and are grouped by concern:

    ### 1. Climate Rules (`ClimateRules/`) — 12 rules

    Adapt note preferences and performance advice to the user's environment.

    | Rule | Condition | Effect |
    |---|---|---|
    | `HotClimatePreferRule` | Climate = Hot | Prefer citrus, aquatic, green, light musk |
    | `HotClimateAvoidRule` | Climate = Hot | Avoid heavy/warm notes |
    | `HotClimateSillageRule` | Climate = Hot | Set sillage = light |
    | `HotClimateLongevityRule` | Climate = Hot | Flag longevity concern |
    | `ColdClimatePreferRule` | Climate = Cold | Prefer amber, wood, oriental, spice |
    | `ColdClimateSillageRule` | Climate = Cold | Set sillage = strong |
    | `ColdClimateLongevityRule` | Climate = Cold | Favour long-lasting base notes |
    | `HumidClimatePreferRule` | Climate = Humid | Prefer aromatic, woody, iso_e_super |
    | `MixedClimateRule` | Climate = Mixed | Versatile/balanced notes |
    | `DrySkinRule` | SkinType = Dry | Boost longevity, prefer richer bases |
    | `OilySkinRule` | SkinType = Oily | Prefer lighter concentration |
    | `AllYearSeasonRule` | SeasonPreference = AllYear | Versatile notes preferred |

    ### 2. Occasion Rules (`OccasionRules/`) — 10 rules

    Match notes to the social context where the fragrance will be worn.

    | Rule | Preferred Notes |
    |---|---|
    | `OfficePreferRule` | Clean, fresh, professional (moderate projection) |
    | `OfficeSillageRule` | Light sillage — office-friendly |
    | `OfficeSweetnessRule` | Avoid heavy sweetness in office settings |
    | `OfficeLongevityRule` | Moderate longevity — performance rules |
    | `DatePreferRule` | Warm, intimate, sexy, vanilla, amber |
    | `NightlifePreferRule` | Bold, intense, dark, smoky |
    | `NightlifeSillageRule` | Strong sillage for nightlife |
    | `FormalPreferRule` | Classic, timeless, elegant |
    | `SportPreferRule` | Fresh, clean, energetic |
    | `DailyPreferRule` | Light, accessible, versatile |
    | `GymPreferRule` | Minimal, fresh, non-intrusive |

    ### 3. Persona Rules (`PersonaRules/`) — 10 rules

    Map personality archetypes to signature olfactive families.

    | Rule | Preferred Signature |
    |---|---|
    | `CorporatePersonaRule` | Professional, clean, structured |
    | `SexyPersonaRule` | Sensual, warm, provocative |
    | `SportyPersonaRule` | Fresh, energetic, clean |
    | `ArtisticPersonaRule` | Chypre, leather, incense, niche |
    | `MinimalistPersonaRule` | Soft, understated, skin-scent |
    | `RebelliousPersonaRule` | Leather, smoke, avant-garde |
    | `ElegantPersonaRule` | Floral, powdery, timeless |
    | `YouthfulPersonaRule` | Fruity, bright, playful |
    | `MaturePersonaRule` | Deep, complex, balanced |
    | `MysteriousPersonaRule` | Oud, dark woods, resinous |

    ### 4. Sensitivity Rules (`SensitivityRules/`) — 6 rules

    Enforce hard exclusions for users with olfactive sensitivities.

    | Rule | Avoided Notes |
    |---|---|
    | `MigraineSensitivityRule` | Oud, heavy amber, animalic, heavy incense |
    | `HatesSweetSensitivityRule` | Sweet, gourmand, caramel, vanilla-heavy |
    | `HatesSpiceSensitivityRule` | Pepper, spice, cinnamon |
    | `HatesFloralSensitivityRule` | Rose, jasmine, floral bouquets |
    | `HatesFreshSensitivityRule` | Citrus, aquatic, green |
    | `PrefersMinimalSensitivityRule` | Avoids intense/heavy compositions |

    ### 5. Performance Rules (`PerformanceRules/`) — 6 rules

    Fine-tune projection and longevity constraints.

    | Rule | Effect |
    |---|---|
    | `WantsLongPerformanceRule` | Set longevity `>= medium` |
    | `NightlifeLongevityRule` | Nightlife contexts require long longevity |
    | `OfficeLongevityRule` | Office contexts prefer moderate longevity |
    | `DrySkinProjectionRule` | Dry skin reduces projection; prefer richer formulas |
    | `HotClimateTopNotesRule` | In hot climates, top notes evaporate fast; prefer stable mid/base |
    | `MuskAmberVanillaBaseRule` | Musk, amber, vanilla bases anchor longevity |

    ### 6. Compliment Rules (`ComplimentRules/`) — 6 rules

    Cross-dimension rules that combine compliment desire with occasion to fine-tune the profile.

    | Rule | Condition | Effect |
    |---|---|---|
    | `ComplimentDateRule` | Compliment=Yes + Date | Reinforce warm, intimate, vanilla |
    | `ComplimentNightlifeRule` | Compliment=Yes + Nightlife | Amplify bold, projection-heavy choices |
    | `ComplimentWorkRule` | Compliment=Yes + Office | Prefer subtle but memorable signature |
    | `ComplimentNoAvoidHeavyRule` | Compliment=No | Avoid heavy sillage compositions |
    | `ComplimentNeutralRule` | Compliment=Neutral | Balanced, no strong bias |
    | `ComplimentYesAvoidIntimateRule` | Compliment=Yes | Avoid skin-scent / intimate-only notes |

    ---

    ## Data Flow (End-to-End)

    ```
    Client (JSON)
    {
    "climate": 0,           // Hot
    "occasion": 2,          // Date
    "skinType": 0,          // Dry
    "compliment": 0,        // Yes
    "seasonPreference": 4,  // AllYear
    "persona": 1,           // Sexy
    "sensitivity": 0,       // None
    "wantsLongPerformance": true
    }
        │
        ▼   POST /api/ai/expert-system/expert-test
        │
    ExpertSystemChatController.Test()
        │
        ▼
    UserProfileMapper.ToDomain(dto)   →  UserProfile (domain object)
        │
        ▼
    ExpertSystemEngine.Run(profile)
        │
        NRules working memory:
            - UserProfile { Climate=Hot, Occasion=Date, Persona=Sexy, ... }
            - Recommendation { Prefer=[], Avoid=[], ... }
        │
        Matching rules fire (example for this profile):
            ✓ HotClimatePreferRule     → Prefer += [citrus, aquatic, green, light_musk]
            ✓ HotClimateSillageRule    → Sillage = "light"
            ✓ HotClimateAvoidRule      → Avoid  += [heavy, oriental_heavy]
            ✓ DatePreferRule           → Prefer += [warm, intimate, sexy, vanilla, amber]
            ✓ SexyPersonaRule          → Prefer += [sensual, warm, ...]
            ✓ ComplimentDateRule       → Prefer += [warm, intimate, vanilla]
            ✓ WantsLongPerformanceRule → Longevity = ">= medium"
            ✓ DrySkinRule              → (longevity/projection adjustment)
            ✓ AllYearSeasonRule        → (versatile notes)
        │
        ▼
    Recommendation {
    Prefer:    { "citrus", "aquatic", "warm", "intimate", "sexy", "vanilla", "amber", ... },
    Avoid:     { "heavy", "oriental_heavy", ... },
    Sillage:   "light",
    Longevity: ">= medium",
    Reasons:   [ "Hot climate → prefer citrus...", "Date occasion → warm...", ... ]
    }
        │
        ▼
    200 OK (JSON)
    ```

    ---

    ## Service Registration

    The service is registered as a **Singleton** in `Program.cs`, meaning the NRules session factory (compiled rule network) is built only once at application startup and reused across all requests:

    ```csharp
    builder.Services.AddSingleton<IExpertSystemService, ExpertSystemService>();
    ```

    This is correct and intentional: NRules session factories are thread-safe and expensive to build; individual sessions (created per request inside `Run()`) are cheap and not shared.

    ---

    ## Adding a New Rule

    1. Create a new `.cs` file in the relevant `Rules/<Category>/` folder.
    2. Inherit from `NRules.Fluent.Dsl.Rule`.
    3. Override `Define()` — pattern-match on `UserProfile`, bind `Recommendation`, apply changes in `Then()`.
    4. Always call `rec.Reasons.Add(...)` to keep the system explainable.
    5. No registration needed — NRules auto-discovers all `Rule` subclasses in the assembly.

    ```csharp
    public class MyNewRule : Rule
    {
        public override void Define()
        {
            UserProfile user = null!;
            Recommendation rec = null!;

            When()
                .Match(() => user, u => u.MyCondition == SomeValue)
                .Match(() => rec);

            Then()
                .Do(_ =>
                {
                    rec.Prefer.Add("my_note");
                    rec.Reasons.Add("MyCondition → prefer my_note");
                });
        }
    }
    ```

    ---

    ## Summary of Rule Inventory

    | Category | # Rules | Dimension |
    |---|---|---|
    | Climate & Skin | 12 | Environment, skin chemistry, season |
    | Occasion | 10 | Social context / event type |
    | Persona | 10 | Personality archetype |
    | Sensitivity | 6 | Olfactive intolerances (hard exclusions) |
    | Performance | 6 | Longevity & projection constraints |
    | Compliment | 6 | Cross-dimension: compliment desire × occasion |
    | **Total** | **50** | |
