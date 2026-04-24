# Quality Assurance Plan — ALOud
## AI-Powered Perfume E-Commerce Platform

| | |
|---|---|
| **Document** | QAP-ALOud-001 |
| **Version** | 1.0 |
| **Status** | Active |
| **Authors** | Marouane El Moujahid & Bouskri Abdellah |
| **Standards** | ISO 9001:2015 · ISO/IEC 25010:2023 |

> *Quality in ALOud is built through short feedback loops, automated testing, and clean architecture — not documentation bureaucracy. Every rule in this document is backed by a tool or a daily habit. The goal is to ship a reliable, AI-powered product fast, without compromising correctness.*

---

## Table of Contents

1. [Project Snapshot](#1-project-snapshot)
2. [Applicable Standards](#2-applicable-standards)
3. [Quality Objectives & KPIs](#3-quality-objectives--kpis)
4. [Architecture & SOLID Compliance](#4-architecture--solid-compliance)
5. [Naming Conventions & Code Standards](#5-naming-conventions--code-standards)
6. [TDD & Development Flow](#6-tdd--development-flow)
7. [CI/CD Pipeline](#7-cicd-pipeline)
8. [Domain Model & Order Flow Events](#8-domain-model--order-flow-events)
9. [Non-Conformance, Risks & Release](#9-non-conformance-risks--release)
10. [Requirements & Test Traceability Matrix](#10-requirements--test-traceability-matrix)
11. [Environment Configuration](#11-environment-configuration)
12. [Quality Records](#12-quality-records)

---

## 1. Project Snapshot

ALOud is an AI-powered e-commerce platform specialising in perfumes. It combines a dual-surface ASP.NET Core 8 backend (REST API + MVC/Razor Pages) with a Vue.js SPA for admin management, and an AI/ML layer for personalised recommendations using a Qdrant vector database and an expert recommendation system.

| Component | Details |
|---|---|
| **Backend** | ASP.NET Core 8 · dual surface: REST API (`Controllers/Api/`) + MVC Razor Pages (`Controllers/MVC/`) |
| **Admin SPA** | Vue.js · consumed via REST API · role-based access |
| **Database** | SQL Server (EF Core 8 / Migrations) |
| **AI / Vector Store** | Qdrant · perfume embeddings · semantic search |
| **Cache** | Redis · session store · query caching |
| **Auth** | JWT · HTTP-only cookies · role-based middleware |
| **Expert System** | Hybrid recommendation engine (AI + rule-based) |
| **Background Jobs** | Hosted services (`Services/Infrastructure/Jobs/`) |
| **Testing** | xUnit + Moq |
| **Static Analysis** | SonarQube (coverage · complexity · security) |
| **CI/CD** | GitHub Actions · sequential gates |
| **Team** | Marouane El Moujahid · Bouskri Abdellah |
| **Method** | Agile · TDD · short iterations |

**E-Commerce Flow (main pipeline):**

```
Browse → Recommend → Cart → Checkout → Order → Dispatch
  1          2         3        4         5        6
```

---

## 2. Applicable Standards

### 2.1 ISO 9001:2015 — Relevant Clauses

| Clause | Title | How it applies |
|---|---|---|
| 4.4 | QMS Processes | CI/CD pipeline defines, executes, and monitors all process flows |
| 6.1 | Risk & Opportunities | Risk register maintained in Section 9 |
| 7.5 | Documented Info | This QAP, test reports, ADRs, changelogs, SonarQube dashboard |
| 8.3 | Design & Development | TDD: Red → Green → Refactor is the design and validation cycle |
| 8.7 | Nonconforming Outputs | Bug severity classification + corrective action (Section 9) |
| 9.1 | Performance Monitoring | KPI table in Section 3; SonarQube dashboard; CI coverage reports |
| 10.2 | Nonconformity / CA | Every bug gets a regression test before the fix |

### 2.2 ISO/IEC 25010:2023 — Quality Characteristics

| Characteristic | Sub-characteristic | Measurement |
|---|---|---|
| Functional Suitability | Completeness, Correctness | Acceptance tests; TDD suite green; Razor Pages render correctly |
| Reliability | Fault Tolerance | Error handling in integration tests; Result pattern used throughout |
| Performance Efficiency | Time Behaviour | API p95 latency < 300 ms; Redis cache hit rate > 80% |
| Security | Confidentiality, Integrity | OWASP checklist; HTTP-only cookie JWT; SonarQube security gate |
| Maintainability | Modularity, Testability | SOLID review; SonarQube cognitive complexity; clean layer separation |
| Compatibility | Interoperability | REST API contract tests; Vue.js SPA integration |
| Usability | Operability | Admin SPA UX walkthrough; Razor Pages manual review |
| AI Quality | Correctness of outputs | Recommendation relevance; Qdrant embedding quality metrics |

---

## 3. Quality Objectives & KPIs

| Objective | Min | Target | Tool |
|---|---|---|---|
| Unit test coverage — .NET backend | 80% | 90% | `xUnit` + `Cobertura` |
| Unit test coverage — Vue.js SPA | 80% | 85% | `Vitest` |
| SonarQube Quality Gate | Pass | Pass | `SonarQube` |
| New Critical / Blocker issues | 0 | 0 | `SonarQube` |
| Critical bugs in production | 0 | 0 | Issue tracker |
| API response time p95 | < 500 ms | < 300 ms | Application logs |
| Redis cache hit rate (catalog/search) | 70% | 85% | Redis `INFO stats` |
| JWT cookie auth integrity | 100% | 100% | Integration tests |
| Recommendation relevance score | N/A | tracked | Qdrant query logs |
| Order flow completion (no errors) | 100% | 100% | E2E acceptance tests |
| EF Core migration success rate | 100% | 100% | CI migration runner |

---

## 4. Architecture & SOLID Compliance

### 4.1 Backend — Dual-Surface ASP.NET Core 8

```
REST API surface:   Controller → Service → Repository Interface ← Repository (EF Core)
MVC/Razor surface:  Controller → Service → ViewModel → View (.cshtml)
Shared:             Domain Models · DTOs · UnitOfWork · Validation Attributes
```

- **`Controllers/Api/`** — HTTP only: routing, input validation (DTOs + Validation attributes), response serialisation. Zero business logic. Inherits from `BaseApiController`.
- **`Controllers/MVC/`** — Razor Page controllers; render server-side Views. Share the same service layer as the API surface.
- **`Services/Business/`** — One service per domain entity (Perfume, Order, Cart, User, …). Orchestrates domain rules. Depends on interfaces, never on EF Core directly.
- **`Services/Infrastructure/`** — Cross-cutting concerns: Auth, Cache (Redis), Email, Rag (retrieval), LLM config, Jobs.
- **`Repositories/`** — `IRepository<T>` + `IUnitOfWork` defined as interfaces. `Repository<T>` and `UnitOfWork` are the EF Core implementations (SOLID-D).
- **`Services/Infrastructure/ExpertSystem/`** — Hybrid AI recommendation; wraps Qdrant calls and rule-based scoring. Accessed through an interface; `MockExpertSystem` used in tests (SOLID-L).

### 4.2 Vue.js — Admin SPA

```
Vue Component → Composable / Store (Pinia) → API Service → Backend REST API
```

The Vue SPA consumes the same REST API scoped under `/api/v1/`. Role-based routes are enforced both in the Vue router (optimistic) and in the backend middleware (authoritative).

### 4.3 Session & Auth Security

JWT issued on login, delivered via `Set-Cookie` with `HttpOnly; Secure; SameSite=Strict`. Role claims validated on every request by ASP.NET Core policy middleware. Token invalidated on logout and admin-forced role change.

### 4.4 SOLID Pre-Merge Checklist

> Run this before every merge request.

| Principle | Question |
|---|---|
| **S** | Does each class / service / component have exactly one reason to change? |
| **O** | Can new behaviour be added without modifying existing files? |
| **L** | Does `MockExpertSystem` pass all the same tests as the real one? |
| **I** | Are interfaces small? Do controllers import only the service they need? |
| **D** | Is there any `new ConcreteX()` inside a service class? (there must not be) |

---

## 5. Naming Conventions & Code Standards

### 5.1 Git — Branch Naming

| Pattern | Example |
|---|---|
| `feat/<ticket>-short-desc` | `feat/42-perfume-hybrid-recommendation` |
| `fix/<ticket>-short-desc` | `fix/17-cart-quantity-overflow` |
| `hotfix/<ticket>-short-desc` | `hotfix/99-jwt-cookie-expiry-crash` |
| `refactor/<short-desc>` | `refactor/expert-system-interface` |
| `chore/<short-desc>` | `chore/update-sonarqube-config` |
| `test/<short-desc>` | `test/order-service-edge-cases` |

### 5.2 Git — Commit Messages (Conventional Commits)

**Format:** `<type>(<scope>): <short imperative description>`

| Type | Example |
|---|---|
| `feat` | `feat(perfume): add qdrant semantic search endpoint` |
| `fix` | `fix(cart): handle out-of-stock race condition` |
| `test` | `test(order-service): add integration test for checkout flow` |
| `refactor` | `refactor(auth): extract jwt cookie builder into service` |
| `chore` | `chore(ci): raise coverage gate to 85%` |
| `docs` | `docs(qap): add naming conventions section` |
| `perf` | `perf(catalog): add redis cache for perfume listing` |
| `style` | `style(views): align checkout form layout` |

### 5.3 .NET Backend (C#)

| Element | Convention | Example |
|---|---|---|
| Files | `PascalCase.cs` | `PerfumeService.cs`, `OrderController.cs` |
| Test files | `<Class>Tests.cs` | `PerfumeServiceTests.cs` |
| Interfaces | `I<Name>` | `IPerfumeService`, `IUnitOfWork` |
| Classes / Records | PascalCase | `PerfumeService`, `OrderDto` |
| Methods | PascalCase | `GetByIdAsync()`, `CreateOrderAsync()` |
| Private fields | `_camelCase` | `_perfumeRepo`, `_cache` |
| Constants | PascalCase | `MaxCartItems`, `DefaultCacheTtl` |
| Result type | `Result<T>` | `Result<PerfumeDto>`, `Result.Fail(...)` |
| Test methods | `Method_Scenario_Expected` | `CreateOrder_OutOfStock_ReturnsError` |
| DTOs | `<Entity>Dto` | `PerfumeDto`, `CreateUserDto` |
| ViewModels | `<Name>VM` | `CartItemVM`, `PerfumeCatalogViewModel` |

### 5.4 Vue.js — Admin SPA

| Element | Convention | Example |
|---|---|---|
| Component files | `PascalCase.vue` | `PerfumeTable.vue`, `OrderPanel.vue` |
| Composable files | `use<Name>.ts` | `usePerfume.ts`, `useOrders.ts` |
| Store files | `<name>.store.ts` | `auth.store.ts`, `perfume.store.ts` |
| Service files | `<name>.service.ts` | `perfume.service.ts` |
| Type files | `<name>.types.ts` | `perfume.types.ts` |
| Test files | `<name>.spec.ts` | `PerfumeTable.spec.ts` |
| Variables | camelCase | `perfumeList`, `isLoading` |
| Constants | `SCREAMING_SNAKE_CASE` | `API_BASE_URL`, `MAX_PAGE_SIZE` |

---

## 6. TDD & Development Flow

> **The only rule:** No production code without a failing test first. **Red → Green → Refactor.** SonarQube on Refactor. Commit only on green.

### 6.1 Three Test Levels — The Pyramid

- **Unit** — single class or method; all dependencies mocked via Moq + interfaces. Very fast; run on every save. *(xUnit, Moq, FluentAssertions)*
- **Integration** — `BaseApiController` + real EF Core + in-memory or SQL Server test DB + `MockExpertSystem`. Validates the full service-repository-database stack. Run on every push. *(xUnit, DbContextFactory)*
- **Acceptance** — one scenario per user story; validates the full vertical slice (API endpoint to DB). Uses the `post-man-endpoints.json` smoke test collection as input for automated runs.

### 6.2 Branching Strategy

- **`main`** — protected; production-ready and deployable at all times
- **`develop`** — integration branch; all features land here after CI passes
- **`feat/*`** — short-lived; one per story; merged when CI is fully green
- **`hotfix/*`** — emergency production patch; merged into both `main` and `develop`

### 6.3 Database Migrations

Managed with EF Core Migrations (`/Migrations/`). Every `dotnet ef migrations add` is immediately validated with a `dotnet ef database update` in CI. No manual schema changes ever. Migration rollback plan: `dotnet ef database update <previous>`.

---

## 7. CI/CD Pipeline

Triggered on every push and pull request to `develop` and `main`. Stages are strictly sequential — any failure stops the pipeline and blocks the merge.

| # | Stage | Actions | Blocks if… |
|---|---|---|---|
| 1 | Build | `dotnet build` · Vue.js production build | Compilation error |
| 2 | Lint | `dotnet format --verify-no-changes` · ESLint · Vue lint | Any lint error (zero tolerance) |
| 3 | Unit Tests | `dotnet test` (xUnit) · Vitest | Any test failure |
| 4 | Coverage Gate | Cobertura XML parsed per component | Any component below 80% |
| 5 | Integration | SQL Server test DB + `MockExpertSystem` + EF migrations | Any test failure |
| 6 | SonarQube | Static analysis · duplication · security hotspots | Quality gate fails |
| 7 | Deploy Dev | Auto-deploy to development environment | Health-check error |
| 8 | Deploy Prod | **Manual trigger** — approval required | No manual approval |

**SonarQube Quality Gate: ALOud-Gate**

- Coverage on new code ≥ 80%
- Duplication ≤ 3%
- Maintainability / Reliability / Security ratings = A
- All hotspots reviewed
- Zero new Critical or Blocker issues

---

## 8. Domain Model & Order Flow Events

Every significant state transition in the order and AI recommendation flow is observable through the service layer and covered by a mandatory test scenario.

| Flow Stage | Events / Transitions | Mandatory Test Scenarios |
|---|---|---|
| 1 — Browse & Search | Catalog load · Qdrant semantic search · Filter by family/accord/note | Search returns relevant results; empty query returns all; invalid filter returns 400 |
| 2 — Recommendation | Expert system invoked · Hybrid score computed · Result ranked | Rejected/low-score perfume not returned; result tied to user profile |
| 3 — Cart | Item added · Quantity updated · Item removed · Stock reserved | Out-of-stock item cannot be added; cart persists across sessions |
| 4 — Checkout | Address validated · Payment initiated · Stock locked · `CheckoutAddress` created | Invalid address rejected; payment failure rolls back stock lock |
| 5 — Order Created | `Order` + `OrderItem` records created · `OrderStatusHistory` appended | Order links correct cart items; history entry immutable |
| 6 — Dispatch | `OrderShipment` + `OrderShipmentItem` recorded · Status updated | Shipment references correct order; customer reference stored |

---

## 9. Non-Conformance, Risks & Release

### 9.1 Bug Severity & Response Time

| Severity | Definition | Response |
|---|---|---|
| **🔴 Critical** | Data loss · security breach (JWT leak, cookie misconfiguration) · order corruption · system down | Open hotfix branch immediately; deploy same day |
| **🟠 Major** | Feature broken · checkout failure · recommendation engine returning wrong results · AI/Qdrant sync failing | Fix within current or next feature cycle |
| **⚪ Minor** | UI defect · cosmetic · Razor Pages rendering glitch · non-blocking | Log it; fix when a slot is available |

> **Corrective Action Rule:** Write a failing test that reproduces the bug **before** writing the fix. The test becomes the permanent regression guard. This is TDD applied to bug fixing.

### 9.2 Risk Register

| Risk | Level | Mitigation |
|---|---|---|
| JWT cookie misconfiguration allows auth bypass | High | Integration test asserts `HttpOnly`, `Secure`, `SameSite=Strict` on every login response |
| Qdrant vector DB unavailable | High | Fallback to rule-based recommendation; health-check endpoint; service returns graceful 503 |
| SQL Server schema drift (EF Core migration missing) | High | CI runs `dotnet ef database update` on every push; migration snapshot checked in version control |
| Dual-surface logic duplication (MVC vs API) | Med | Shared service layer is the single source of truth; controllers must not contain business logic |
| Redis unavailability (cache + session) | Med | Redis persistence enabled; graceful degradation to DB queries on cache miss; 503 health check |
| Coverage drops below 80% | Med | CI quality gate hard-blocks any merge until coverage is restored |
| Scope creep on AI/Expert System | Low | `IExpertSystemService` interface freezes the contract; new strategies = new implementation class |

### 9.3 Definition of Done

A story is **Done** when *all* of the following are true:

1. All acceptance criteria implemented as automated tests — and passing
2. Code coverage ≥ 80% on every affected component
3. SonarQube quality gate passes (no new issues, all ratings A)
4. SOLID checklist reviewed and signed off
5. CI pipeline fully green on `develop`
6. If the story touches an API endpoint: Postman collection updated
7. If the story touches a Razor Page: manual render verified in browser

### 9.4 Production Release Gate

Before deploying to production, all of the following must be confirmed:

- [ ] All stories in the release satisfy the Definition of Done
- [ ] Database backup taken (`backup-databases.sh` executed)
- [ ] All EF Core migrations tested in development environment first
- [ ] Expert system & Qdrant smoke test executed and passed
- [ ] JWT cookie security integration test green
- [ ] All SonarQube security hotspots reviewed / resolved
- [ ] Rollback plan documented (migration rollback + previous binary)
- [ ] Release notes written and committed to `CHANGELOG.md`

---

## 10. Requirements & Test Traceability Matrix

| Req. ID | Requirement | Test Scope | Status |
|---|---|---|---|
| REQ-01 | Perfume catalog browse with pagination and search | Unit + Integration + Acceptance | Planned |
| REQ-02 | Qdrant semantic search (embedding-based) | Integration (Qdrant mock) | Planned |
| REQ-03 | Hybrid expert recommendation (AI + rule-based) | Unit (domain rule) + Integration | Planned |
| REQ-04 | User registration, email verification, login | Unit + Integration + Acceptance | **Ongoing** |
| REQ-05 | JWT via HTTP-only cookie — create / refresh / invalidate | Integration (cookie assertion) | **Ongoing** |
| REQ-06 | Role-based access control (Customer / Admin) | Unit + Integration (middleware) | **Ongoing** |
| REQ-07 | Cart management (add, update qty, remove, persist) | Unit + Integration | Planned |
| REQ-08 | Checkout flow (address, payment, stock lock) | Unit + Integration + Acceptance | Planned |
| REQ-09 | Order creation with status history (append-only) | Unit + Integration | Planned |
| REQ-10 | Order dispatch and shipment recording | Unit + Integration | Planned |
| REQ-11 | Admin dashboard stats (sales, stock, users) | Unit (service) + Integration | Planned |
| REQ-12 | Perfume CRUD via Admin SPA (Vue.js) | Unit (service) + Acceptance | Planned |
| REQ-13 | Redis cache for catalog and search results | Integration (cache hit/miss) | Planned |
| REQ-14 | Background jobs (stock sync, email queue) | Unit + Integration | Planned |
| REQ-15 | MVC Razor Pages (browse, cart, checkout, orders) | Manual render + smoke test | **Ongoing** |
| REQ-16 | LLM / RAG chat assistant (AI cart, expert chat) | Unit (mock LLM) + Integration | Planned |
| REQ-17 | Address management (add, default, delete) | Unit + Integration | Planned |
| REQ-18 | Stock reservation and release on order cancel | Unit (domain rule) | Planned |
| REQ-19 | EF Core migration versioning and rollback | CI migration runner | **Ongoing** |
| REQ-20 | CI/CD pipeline with 80% coverage hard gate | Pipeline quality gate (auto) | **Ongoing** |
| REQ-21 | Session cookie security (HttpOnly, Secure, SameSite) | Integration (header assertion) | **Ongoing** |
| REQ-22 | Qdrant & SQL Server DB backup and restore | `backup-databases.sh` + CI | Planned |

---

## 11. Environment Configuration

| Item | Development | Production |
|---|---|---|
| .NET backend | `dotnet run` · hot-reload | Compiled binary; systemd / IIS service |
| Vue.js admin SPA | `vite dev` local server | Static build served by Nginx / CDN |
| SQL Server | Local Docker container | Production SQL Server instance |
| Qdrant | Local Docker container | Production Qdrant instance |
| Redis | Local Docker container | Production Redis with AOF persistence |
| Expert System / LLM | Mock adapter in all tests | Real adapter; mode via env var |
| Secrets | `.env` file (git-ignored) | Environment variables on server |
| Logging | Debug level · stdout | Info/Warn · rotating log files |

> **Security Rule — Non-Negotiable:** No credentials, API keys, connection strings, or Qdrant API keys are ever committed to version control. All secrets are managed via environment variables. `.env` and `appsettings.Development.json` (with real values) are in `.gitignore`.

---

## 12. Quality Records

Kept minimal and automated wherever possible.

| Record | Format | Location |
|---|---|---|
| This QAP | PDF + LaTeX source | Version control (`/docs/qap/`) |
| CI test results | TRX / HTML (`/TestResults/`) | CI artefacts (auto-generated) |
| Coverage reports | Cobertura XML / HTML | CI artefacts (auto-generated) |
| SonarQube analysis | SonarQube dashboard | SonarQube server |
| Architecture Decision Records | Markdown (ADR format) | `/docs/adr/` in repo |
| API documentation | Markdown | `/docs/API/` in repo |
| Admin SPA documentation | Markdown | `/docs/admin-docs/` in repo |
| Release notes / changelog | Markdown (`CHANGELOG.md`) | Version control root |
| DB backups | `.bak` + Qdrant snapshot | `/backups/<date>/` |
| Postman / API smoke tests | JSON collection | `/Tests/post-man-endpoints.json` |
| Bug and issue log | Issue tracker | Project issue tracker (GitHub Issues) |

---

*Living document — update when a module is added, a process changes, or a major nonconformance drives an improvement.*

*QAP-ALOud-001 · v1.0*