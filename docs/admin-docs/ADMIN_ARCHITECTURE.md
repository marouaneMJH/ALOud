# Admin Dashboard - Architecture Overview

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                          ADMIN DASHBOARD                            │
│                      (MVC Layer - Razor Views)                      │
└─────────────────────────────────────────────────────────────────────┘
                                    ↓
        ┌───────────────────────────────────────────────────┐
        │         CONTROLLERS (Route Handlers)              │
        ├───────────────────────────────────────────────────┤
        │ AdminController         │ PerfumeAdminController │
        │ ├─ Index (Dashboard)    │ ├─ Brands (CRUD)      │
        │ ├─ LLMConfig            │ ├─ Perfumes (CRUD)    │
        │ ├─ SwitchProvider       │ ├─ Families (CRUD)    │
        │ └─ ExpertSystem         │ ├─ Notes (CRUD)       │
        │                         │ ├─ Accords (CRUD)     │
        │                         │ ├─ Tags (CRUD)        │
        │                         │ ├─ Seasons (CRUD)     │
        │                         │ └─ Occasions (CRUD)   │
        └───────────────────────────────────────────────────┘
                                    ↓
        ┌───────────────────────────────────────────────────┐
        │          SERVICE LAYER (Business Logic)           │
        ├───────────────────────────────────────────────────┤
        │ ┌─────────────────────────────────────────────┐   │
        │ │  IDashboardService                          │   │
        │ │  ├─ GetDashboardStatsAsync()                │   │
        │ └─────────────────────────────────────────────┘   │
        │ ┌─────────────────────────────────────────────┐   │
        │ │  IBrandService, IPerfumeService, etc.       │   │
        │ │  ├─ GetAllAsync(pageIndex, size, search)   │   │
        │ │  ├─ GetForEditAsync(id)                     │   │
        │ │  ├─ CreateAsync(dto)                        │   │
        │ │  ├─ UpdateAsync(dto)                        │   │
        │ │  ├─ DeleteAsync(id)                         │   │
        │ │  └─ EntityExistsAsync(name)                 │   │
        │ └─────────────────────────────────────────────┘   │
        │ ┌─────────────────────────────────────────────┐   │
        │ │  LLMConfigService                           │   │
        │ │  ├─ GetAvailableProviders()                 │   │
        │ │  └─ SwitchProvider(providerId)              │   │
        │ └─────────────────────────────────────────────┘   │
        └───────────────────────────────────────────────────┘
                                    ↓
        ┌───────────────────────────────────────────────────┐
        │       DATA ACCESS LAYER (EF Core DbContext)       │
        ├───────────────────────────────────────────────────┤
        │ ALOudDbContext                                    │
        │ ├─ Perfumes (DbSet)                               │
        │ ├─ Brands (DbSet)                                 │
        │ ├─ Families (DbSet)                               │
        │ ├─ Notes (DbSet)                                  │
        │ ├─ Accords (DbSet)                                │
        │ ├─ Tags (DbSet)                                   │
        │ ├─ Seasons (DbSet)                                │
        │ └─ Occasions (DbSet)                              │
        └───────────────────────────────────────────────────┘
                                    ↓
        ┌───────────────────────────────────────────────────┐
        │     DATABASE (SQL Server)                         │
        ├───────────────────────────────────────────────────┤
        │ Perfume | Brand | Family | Note | Accord | ...   │
        │ PerfumeNote | PerfumeAccord | PerfumeTag | ...   │
        └───────────────────────────────────────────────────┘
```

## Data Flow Diagram (CRUD Example: Create Perfume)

```
┌──────────────────────────────────────────────────────────────────┐
│ 1. USER INTERACTION                                              │
│    Click "Add Perfume" → GET /Admin/CreatePerfume                │
└──────────────────────────────────────────────────────────────────┘
                                ↓
┌──────────────────────────────────────────────────────────────────┐
│ 2. CONTROLLER (PerfumeAdminController.CreatePerfume)             │
│    ├─ Populate ViewBag with dropdown data:                      │
│    │  ├─ Brands                                                  │
│    │  ├─ Families                                                │
│    │  ├─ Notes                                                   │
│    │  ├─ Accords                                                 │
│    │  ├─ Tags                                                    │
│    │  ├─ Seasons                                                 │
│    │  └─ Occasions                                               │
│    └─ Return View with CreatePerfumeDto model                    │
└──────────────────────────────────────────────────────────────────┘
                                ↓
┌──────────────────────────────────────────────────────────────────┐
│ 3. RENDER FORM (Views/Admin/Perfumes/Create.cshtml)             │
│    ├─ Basic Info Section                                         │
│    ├─ Scent Profile Section                                      │
│    ├─ Classification Section                                     │
│    └─ Related Items Section                                      │
└──────────────────────────────────────────────────────────────────┘
                                ↓
┌──────────────────────────────────────────────────────────────────┐
│ 4. USER SUBMITS FORM                                             │
│    POST /Admin/CreatePerfume with CreatePerfumeDto               │
│    Include: @Html.AntiForgeryToken()                             │
└──────────────────────────────────────────────────────────────────┘
                                ↓
┌──────────────────────────────────────────────────────────────────┐
│ 5. SERVER-SIDE VALIDATION                                        │
│    ├─ [ValidateAntiForgeryToken] checks token                    │
│    ├─ ModelState.IsValid checks all attributes                   │
│    ├─ IValidatableObject checks relationships                    │
│    └─ Check for duplicate names                                  │
└──────────────────────────────────────────────────────────────────┘
                                ↓
┌──────────────────────────────────────────────────────────────────┐
│ 6. SERVICE LAYER (IPerfumeService)                               │
│    CreatePerfumeAsync(CreatePerfumeDto) executes:                │
│    ├─ Map DTO to Entity                                          │
│    ├─ Create Perfume record                                      │
│    ├─ Create PerfumeNote relationships                           │
│    ├─ Create PerfumeAccord relationships                         │
│    ├─ Create PerfumeBrand association                            │
│    ├─ Create PerfumeFamily relationships                         │
│    ├─ Create PerfumeTag relationships                            │
│    ├─ Create PerfumeSeason relationships                         │
│    ├─ Create PerfumeOccasion relationships                       │
│    └─ SaveChangesAsync() to database                             │
└──────────────────────────────────────────────────────────────────┘
                                ↓
┌──────────────────────────────────────────────────────────────────┐
│ 7. DATABASE OPERATIONS (EF Core)                                 │
│    ├─ INSERT INTO Perfume (...)                                  │
│    ├─ INSERT INTO PerfumeNote (...)                              │
│    ├─ INSERT INTO PerfumeAccord (...)                            │
│    ├─ INSERT INTO PerfumeBrand (...)                             │
│    └─ (All within a transaction)                                 │
└──────────────────────────────────────────────────────────────────┘
                                ↓
┌──────────────────────────────────────────────────────────────────┐
│ 8. REDIRECT & FEEDBACK                                           │
│    ├─ Set TempData["Success"] = "Perfume created successfully"   │
│    ├─ RedirectToRoute("MvcPerfumeAdminPerfumes")                 │
│    └─ Return view with success message                           │
└──────────────────────────────────────────────────────────────────┘
```

## Request/Response Flow Diagram

```
USER BROWSER                        WEB SERVER                    DATABASE
     │                                   │                             │
     ├─ GET /Admin ─────────────────────>│                             │
     │                                   ├─ IDashboardService         │
     │                                   ├──────────────────────────>│
     │                                   │ SELECT TOP 5 BY COUNT      │
     │                                   │<─ Get Stats ───────────────┤
     │                                   │ Returns DashboardStatsDto  │
     │<─ HTML Dashboard ─────────────────┤                             │
     │                                   │                             │
     ├─ Click Add Perfume                │                             │
     ├─ GET /Admin/CreatePerfume ────────>│                             │
     │                                   ├─ PopulatePerfumeViewBag()  │
     │                                   ├──────────────────────────>│
     │                                   │ SELECT * FROM Brands       │
     │                                   │ SELECT * FROM Families     │
     │                                   │ SELECT * FROM Notes        │
     │                                   │ ... etc                    │
     │                                   │<─ Collection Data ─────────┤
     │<─ HTML Form + Dropdowns ──────────┤                             │
     │                                   │                             │
     ├─ Fill Form & Submit               │                             │
     ├─ POST /Admin/CreatePerfume ───────>│                             │
     │                                   ├─ Validate Model            │
     │                                   ├─ Check Duplicates          │
     │                                   ├─ IPerfumeService.Create()  │
     │                                   ├──────────────────────────>│
     │                                   │ INSERT Perfume             │
     │                                   │ INSERT PerfumeNote         │
     │                                   │ INSERT PerfumeAccord       │
     │                                   │ ... etc                    │
     │                                   │<─ Success ─────────────────┤
     │<─ Redirect to List ────────────────┤                             │
     │                                   │                             │
     ├─ GET /Admin/Perfumes ────────────>│                             │
     │                                   ├─ IPerfumeService.GetAll()  │
     │                                   ├──────────────────────────>│
     │                                   │ SELECT * FROM Perfumes     │
     │                                   │ (With pagination & joins)  │
     │                                   │<─ PaginatedList ───────────┤
     │<─ HTML List + Success Toast ──────┤                             │
     │                                   │                             │
```

## Entity Relationship Model (Simplified)

```
Perfume (Master Entity)
    │
    ├─────────────── BrandId ──────────> Brand
    │
    ├─────────────────────────────────> PerfumeFamily ──────> Family
    │                                        (Many-to-Many)
    │
    ├─────────────────────────────────> PerfumeNote ───────> Note
    │                                    (Many-to-Many)
    │                                    └─ NoteLevel (Top|Middle|Base)
    │
    ├─────────────────────────────────> PerfumeAccord ────> Accord
    │                                    (Many-to-Many)
    │                                    └─ Intensity (Strong|Medium|Light)
    │
    ├─────────────────────────────────> PerfumeTag ────────> Tag
    │                                    (Many-to-Many)
    │
    ├─────────────────────────────────> PerfumeSeason ────> Season
    │                                    (Many-to-Many)
    │
    └─────────────────────────────────> PerfumeOccasion ──> Occasion
                                         (Many-to-Many)
```

## CRUD Operation Patterns

```
┌─────────────────────────────────────────────────────────────┐
│ CREATE                                                      │
├─────────────────────────────────────────────────────────────┤
│ 1. GET /Create[Entity]     → Return form with ViewBag data │
│ 2. POST /Create[Entity]    → Validate & Service.Create()   │
│ 3. Redirect to List        → Show success message           │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ READ (List)                                                 │
├─────────────────────────────────────────────────────────────┤
│ 1. GET /[Entity]s          → Service.GetAll(page, search)  │
│ 2. Apply filters           → Chainable LINQ queries         │
│ 3. Return PaginatedList    → Display in table               │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ READ (Details - Perfume only)                              │
├─────────────────────────────────────────────────────────────┤
│ 1. GET /Details/{id}       → Service.GetDetailsAsync(id)   │
│ 2. Fetch with relationships → Include all FK data           │
│ 3. Return DetailedDto      → Display full info              │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ UPDATE                                                      │
├─────────────────────────────────────────────────────────────┤
│ 1. GET /Edit/{id}          → Load existing & Form with data │
│ 2. POST /Edit/{id}         → Validate & Service.Update()   │
│ 3. Redirect to List        → Show success message           │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ DELETE                                                      │
├─────────────────────────────────────────────────────────────┤
│ 1. Click Delete button     → Modal confirmation appears     │
│ 2. POST /Delete/{id}       → Service.Delete(id)             │
│ 3. Redirect to List        → Show success/error message     │
└─────────────────────────────────────────────────────────────┘
```

## Dependency Injection Architecture

```
┌────────────────────────────────────────────────────────────┐
│ Program.cs (DI Container Configuration)                    │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ builder.Services.AddScoped<IDashboardService, ...>()      │
│ builder.Services.AddScoped<IBrandService, ...>()          │
│ builder.Services.AddScoped<IPerfumeService, ...>()        │
│ builder.Services.AddScoped<IFamilyService, ...>()         │
│ builder.Services.AddScoped<INoteService, ...>()           │
│ builder.Services.AddScoped<IAccordService, ...>()         │
│ builder.Services.AddScoped<ITagService, ...>()            │
│ builder.Services.AddScoped<ISeasonService, ...>()         │
│ builder.Services.AddScoped<IOccasionService, ...>()       │
│ builder.Services.AddScoped<LLMConfigService>()            │
│                                                            │
│ builder.Services.AddDbContext<ALOudDbContext>(...)        │
│ builder.Services.AddAuthentication(...)                   │
│                                                            │
└────────────────────────────────────────────────────────────┘
                           ↓
┌────────────────────────────────────────────────────────────┐
│ Controllers (Receive Dependencies via Constructor)         │
├────────────────────────────────────────────────────────────┤
│                                                            │
│ AdminController(                                           │
│     IDashboardService,                                    │
│     LLMConfigService,                                     │
│     ILogger<AdminController>                              │
│ )                                                          │
│                                                            │
│ PerfumeAdminController(                                   │
│     IBrandService,                                        │
│     IPerfumeService,                                      │
│     IFamilyService,                                       │
│     INoteService,                                         │
│     IAccordService,                                       │
│     ITagService,                                          │
│     ISeasonService,                                       │
│     IOccasionService,                                     │
│     IDashboardService,                                    │
│     ILogger<PerfumeAdminController>                       │
│ )                                                          │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

## Request Lifecycle

```
1. USER REQUEST
   ├─ Browser sends HTTP request
   ├─ Route matches "/Admin" prefix
   └─ AuthorizeAttribute checks [Authorize]

2. AUTHENTICATION CHECK
   ├─ Cookie parsed from request
   ├─ User principal extracted
   ├─ If not authenticated → Redirect to login
   └─ If authenticated → Continue

3. CONTROLLER ACTION
   ├─ Dependencies injected from DI container
   ├─ Action method executes
   ├─ Services called
   └─ Database queries executed

4. SERVICE LAYER
   ├─ Business logic applied
   ├─ Data validation performed
   ├─ DbContext operations queued
   └─ SaveChangesAsync() executes

5. DATABASE TRANSACTION
   ├─ EF Core generates SQL
   ├─ SQL Server executes queries
   ├─ Transaction commits/rolls back
   └─ Result returned to service

6. RESPONSE BUILDING
   ├─ View rendered with model data
   ├─ Razor engine processes template
   ├─ HTML generated
   └─ Response sent to browser

7. CLIENT RENDERING
   ├─ Browser receives HTML
   ├─ DOM parsed and rendered
   ├─ CSS applied
   └─ User sees page
```

## Validation Pipeline

```
┌──────────────────────────────────────────────────────────┐
│ 1. CLIENT-SIDE (Browser)                                 │
│    ├─ HTML5 required attribute                           │
│    ├─ Pattern matching for formats                       │
│    └─ User feedback via browser                          │
└──────────────────────────────────────────────────────────┘
                         ↓ (Still POST if bypassed)
┌──────────────────────────────────────────────────────────┐
│ 2. [ValidateAntiForgeryToken]                            │
│    ├─ Check token present in request                     │
│    ├─ Verify token matches session                       │
│    └─ Reject if invalid (CSRF protection)                │
└──────────────────────────────────────────────────────────┘
                         ↓
┌──────────────────────────────────────────────────────────┐
│ 3. [Authorize]                                           │
│    ├─ Check user authenticated                           │
│    └─ Reject if not logged in                            │
└──────────────────────────────────────────────────────────┘
                         ↓
┌──────────────────────────────────────────────────────────┐
│ 4. Model Binding & Data Annotations                      │
│    ├─ [Required] - field must have value                 │
│    ├─ [MaxLength] - string length limits                 │
│    ├─ [Range] - numeric ranges                           │
│    ├─ [Url] - valid URL format                           │
│    └─ Set ModelState.IsValid = false on errors           │
└──────────────────────────────────────────────────────────┘
                         ↓
┌──────────────────────────────────────────────────────────┐
│ 5. ModelState Check                                      │
│    ├─ if (!ModelState.IsValid) return View(dto)          │
│    ├─ Show validation errors on form                     │
│    └─ User corrects and resubmits                        │
└──────────────────────────────────────────────────────────┘
                         ↓
┌──────────────────────────────────────────────────────────┐
│ 6. IValidatableObject.Validate()                         │
│    ├─ Custom validation logic (relationships)            │
│    ├─ Check for duplicate IDs in collections             │
│    ├─ Cross-field validation                             │
│    └─ Yield return ValidationResult on error             │
└──────────────────────────────────────────────────────────┘
                         ↓
┌──────────────────────────────────────────────────────────┐
│ 7. Service-Level Validation                              │
│    ├─ Duplicate name check                               │
│    ├─ Entity exists check                                │
│    ├─ Relationship integrity check                       │
│    └─ Return error if validation fails                   │
└──────────────────────────────────────────────────────────┘
                         ↓
┌──────────────────────────────────────────────────────────┐
│ 8. If All Valid → Database Operation                     │
│    └─ SaveChangesAsync() and transaction commits         │
└──────────────────────────────────────────────────────────┘
```

