# Admin Dashboard - Quick Reference Guide

## Quick Navigation Map

```
/Admin
├── / (Dashboard)
├── /LLMConfig (LLM Provider Configuration)
├── /ExpertSystem (Expert System Configuration)
└── Catalog Management
    ├── /Perfumes (+ Create, Edit, Delete, Details)
    ├── /Brands (+ Create, Edit, Delete)
    ├── /Families (+ Create, Edit, Delete)
    ├── /Notes (+ Create, Edit, Delete)
    ├── /Accords (+ Create, Edit, Delete)
    ├── /Tags (+ Create, Edit, Delete)
    ├── /Seasons (+ Create, Edit, Delete)
    └── /Occasions (+ Create, Edit, Delete)
```

## Controllers at a Glance

| Controller | File | Routes | Purpose |
|-----------|------|--------|---------|
| AdminController | `/Controllers/MVC/AdminController.cs` | Dashboard, LLMConfig, ExpertSystem | System configuration & overview |
| PerfumeAdminController | `/Controllers/MVC/PerfumeAdminController.cs` | All catalog CRUD operations | Perfume catalog management |

## Key Routes Quick Lookup

### Dashboard & Configuration (4 routes)
- `GET /Admin` - Dashboard with KPIs
- `GET /Admin/LLMConfig` - LLM provider selection
- `POST /Admin/SwitchProvider` - Switch active LLM provider
- `GET /Admin/ExpertSystem` - Expert system configuration

### Perfume Management (7 actions × 8 entity types = 56 routes)
**For each entity: Perfume, Brand, Family, Note, Accord, Tag, Season, Occasion**
- `GET /{Entity}s` - List with pagination/search
- `GET /Create{Entity}` - Create form
- `POST /Create{Entity}` - Create submit
- `GET /Edit{Entity}/{id}` - Edit form
- `POST /Edit{Entity}/{id}` - Edit submit
- `POST /Delete{Entity}/{id}` - Delete
- `GET /PerfumeDetails/{id}` - (Perfumes only) Details view

## Data Models Reference

### Input DTOs (Create/Update)
- `CreatePerfumeDto` / `UpdatePerfumeDto` - Complex form with relationships
- `CreateBrandDto` / `UpdateBrandDto` - Simple name field
- (Similar for Family, Note, Accord, Tag, Season, Occasion)

### Output DTOs (Display)
- `DashboardStatsDto` - KPI data for dashboard
- `PerfumeDto` - List view perfume data
- `PerfumeDetailsDto` - Full perfume with all relationships
- `BrandDto`, `FamilyDto`, etc. - Entity lists

## Common Filtering Parameters

| Entity | Filters |
|--------|---------|
| Perfumes | searchTerm, brandId, familyId, genderProfile |
| Notes | searchTerm, category |
| Others | searchTerm (standard search) |

## Validation Rules

### Perfume
- Name: Required, max 300 chars
- Price: Must be positive
- Stock: Must be >= 0
- Intensity: Light, Moderate, Strong, Very Strong
- Longevity: 1-2h, 3-4h, 5-6h, 7-8h, 8+ hours
- Sillage: Intimate, Moderate, Strong, Enormous
- Gender: Male, Female, Unisex, Masculine, Feminine
- Price Range: Budget, Mid-range, Premium, Luxury
- Notes: Top/Middle/Base levels allowed
- Accords: Strong/Medium/Light intensities

### Other Entities
- Name: Required, max 200 chars
- No duplicates allowed

## Services to Know

| Service | Purpose |
|---------|---------|
| IDashboardService | Dashboard statistics aggregation |
| IBrandService | Brand CRUD + statistics |
| IPerfumeService | Perfume CRUD + filtering + details |
| IFamilyService | Family CRUD |
| INoteService | Note CRUD + category management |
| IAccordService | Accord CRUD |
| ITagService | Tag CRUD |
| ISeasonService | Season CRUD |
| IOccasionService | Occasion CRUD |
| LLMConfigService | LLM provider configuration |

## View Structure

```
Views/Admin/
├── Index.cshtml (Dashboard)
├── LLMConfig.cshtml (LLM config)
├── ExpertSystem.cshtml (Expert system)
├── Perfumes/
│   ├── Index.cshtml (List)
│   ├── Create.cshtml (Create form)
│   ├── Edit.cshtml (Edit form)
│   └── Details.cshtml (Full details)
├── Brands/
│   ├── Index.cshtml (List)
│   ├── Create.cshtml (Create form)
│   └── Edit.cshtml (Edit form)
├── Families/ (Same structure as Brands)
├── Notes/ (Same structure as Brands)
├── Accords/ (Same structure as Brands)
├── Tags/ (Same structure as Brands)
├── Seasons/ (Same structure as Brands)
├── Occasions/ (Same structure as Brands)
└── _ViewStart.cshtml (Admin layout config)
```

## Common Workflows

### Add a New Perfume
1. Admin → Dashboard → "Add Perfume" button
2. Fill form: Name, Brand, Price, Stock
3. Add Families, Notes (with levels), Accords (with intensity)
4. Set Gender, Tags, Seasons, Occasions
5. Add Image URL, Description
6. Submit → Validate → Redirect to Perfume list

### Edit Metadata (Brand/Family/etc.)
1. Admin → [Entity] list
2. Search/filter as needed
3. Click Edit pencil icon
4. Modify and submit
5. Redirect to list with confirmation

### Delete with Safety
1. Click trash icon
2. Modal confirmation appears
3. Choose confirm/cancel
4. POST with anti-forgery token
5. Service checks integrity
6. Delete and redirect

### Configure LLM Provider
1. Admin → LLM Configuration
2. See active provider (highlighted)
3. If API key exists on another provider, click "Switch to [X]"
4. Confirm switch
5. Immediate effect on next request

## Important Security Notes

- All admin routes require `[Authorize]`
- All POST/DELETE routes require `[ValidateAntiForgeryToken]`
- Use `@Html.AntiForgeryToken()` in all forms
- Duplicate name checks before save
- Relationships validated for integrity
- URLs validated for image fields

## Pagination & Search Defaults

- **Page Size**: 10 items per page
- **Default Sort**: By creation date (most recent first)
- **Search**: Full-text name search
- **Filters**: Chainable on query string

## Performance Considerations

- Dashboard loads top 5 brands/families
- Recent perfumes shows last 10
- Pagination prevents loading all at once
- ViewBag used for dropdown data efficiency
- Services cache common data

## Error Handling

- **Success**: `TempData["Success"]` message shown
- **Error**: `TempData["Error"]` message shown
- **404**: Returns NotFound() if ID not found
- **Validation**: ModelState errors shown inline on form
- **Duplicate**: Specific error message for name conflicts

## Testing Common Scenarios

1. **Create with missing required fields** → Validation error
2. **Create with duplicate name** → Specific error message
3. **Edit with wrong ID format** → BadRequest
4. **Delete non-existent item** → NotFound
5. **Switch LLM without API key** → Button disabled
6. **Filter perfumes by brand** → List filtered correctly
7. **Paginate to next page** → State preserved

