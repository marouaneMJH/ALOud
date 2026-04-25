# Admin Dashboard SPA Migration - Quick Reference

## Overview

Migration of MVC-based Admin Dashboard to a modern Single Page Application (SPA) that consumes REST APIs.

**Status**: Planning  
**Duration**: 10 weeks  
**Tech Stack**: React 18 + TypeScript + Vite  
**API Base**: `/api/v1/admin/`

---

## Admin Pages Summary

### Total: 29 pages across 9 modules

```
┌─────────────────────────────────────────────────────────┐
│                    ADMIN DASHBOARD SPA                  │
├─────────────────────────────────────────────────────────┤
│ 
│  DASHBOARD (1 page)
│  ├─ Dashboard - /admin/dashboard [KPIs, Stats]
│
│  CATALOG MANAGEMENT (19 pages)
│  ├─ Brands (3 pages)
│  │  ├─ List - /admin/catalog/brands
│  │  ├─ Create - /admin/catalog/brands/create
│  │  └─ Edit - /admin/catalog/brands/:id/edit
│  ├─ Perfumes (4 pages)
│  │  ├─ List - /admin/catalog/perfumes
│  │  ├─ Create - /admin/catalog/perfumes/create
│  │  ├─ Edit - /admin/catalog/perfumes/:id/edit
│  │  └─ Details - /admin/catalog/perfumes/:id
│  ├─ Families (3 pages)
│  │  ├─ List - /admin/catalog/families
│  │  ├─ Create - /admin/catalog/families/create
│  │  └─ Edit - /admin/catalog/families/:id/edit
│  ├─ Notes (3 pages)
│  │  ├─ List - /admin/catalog/notes
│  │  ├─ Create - /admin/catalog/notes/create
│  │  └─ Edit - /admin/catalog/notes/:id/edit
│  ├─ Accords (3 pages)
│  │  ├─ List - /admin/catalog/accords
│  │  ├─ Create - /admin/catalog/accords/create
│  │  └─ Edit - /admin/catalog/accords/:id/edit
│  ├─ Tags (3 pages)
│  │  ├─ List - /admin/catalog/tags
│  │  ├─ Create - /admin/catalog/tags/create
│  │  └─ Edit - /admin/catalog/tags/:id/edit
│  └─ Seasons (3 pages)
│     ├─ List - /admin/catalog/seasons
│     ├─ Create - /admin/catalog/seasons/create
│     └─ Edit - /admin/catalog/seasons/:id/edit
│
│  SETTINGS (3 pages)
│  ├─ Expert System - /admin/settings/expert-system
│  ├─ LLM Config - /admin/settings/llm-config
│  └─ System Settings - /admin/settings/system
│
└─────────────────────────────────────────────────────────┘
```

---

## API Endpoints

### Dashboard
```
GET  /api/v1/admin/dashboard/stats
```

### Catalog Management
```
BRANDS:         Brands are managed via /api/v1/admin/brands
PERFUMES:       Perfumes via /api/v1/admin/perfumes
FAMILIES:       Families via /api/v1/admin/families
NOTES:          Notes via /api/v1/admin/notes
ACCORDS:        Accords via /api/v1/admin/accords
TAGS:           Tags via /api/v1/admin/tags
SEASONS:        Seasons via /api/v1/admin/seasons
OCCASIONS:      Occasions via /api/v1/admin/occasions

Standard CRUD Pattern:
GET    /{resource}?page=1&pageSize=10&search=
GET    /{resource}/{id}
POST   /{resource}
PUT    /{resource}/{id}
DELETE /{resource}/{id}
```

### Expert System
```
POST /api/v1/ai/expert-system/test
GET  /api/v1/admin/expert-system/config
PUT  /api/v1/admin/expert-system/config
```

### LLM Configuration
```
GET  /api/v1/admin/llm-providers
GET  /api/v1/admin/llm-providers/current
POST /api/v1/admin/llm-providers/switch
PUT  /api/v1/admin/llm-providers/{id}/config
```

---

## Component Hierarchy

```
Layout
├── Sidebar
│   └── Navigation Links
├── Header
│   └── User Menu
└── Main Content
    ├── Dashboard Page
    │   ├── StatsCard
    │   ├── Chart
    │   └── ActivityFeed
    ├── List Pages
    │   ├── SearchBar
    │   ├── FilterBar
    │   ├── DataTable
    │   │   └── TableRow (Edit, Delete actions)
    │   └── Pagination
    ├── Form Pages
    │   ├── FormSection
    │   ├── FormField
    │   │   ├── TextField
    │   │   ├── TextareaField
    │   │   ├── SelectField
    │   │   ├── MultiSelectField
    │   │   ├── DateField
    │   │   ├── ColorPickerField
    │   │   └── SliderField
    │   └── FormActions (Submit, Cancel, Delete)
    ├── Details Pages
    │   ├── InfoSection
    │   ├── TabsSection
    │   └── RelatedItems
    └── Settings Pages
        ├── ConfigForm
        └── ProviderCards
```

---

## Shared Components

### Common Components (Reusable)

| Component | Usage |
|-----------|-------|
| `<Layout>` | Every page |
| `<DataTable>` | All list pages |
| `<FormField>` | All form pages |
| `<Pagination>` | All list pages |
| `<Modal>` | Confirmations, forms |
| `<Toast>` | Notifications |
| `<LoadingSpinner>` | Data loading |
| `<SearchBar>` | Search functionality |
| `<FilterBar>` | Advanced filtering |

### Specific Form Components

| Component | Pages |
|-----------|-------|
| `<BrandForm>` | Brand Create/Edit |
| `<PerfumeForm>` | Perfume Create/Edit |
| `<FamilyForm>` | Family Create/Edit |
| `<NoteForm>` | Note Create/Edit |
| `<AccordForm>` | Accord Create/Edit |
| `<TagForm>` | Tag Create/Edit |
| `<SeasonForm>` | Season Create/Edit |
| `<OccasionForm>` | Occasion Create/Edit |

---

## Feature Matrix

| Page | Search | Filter | Pagination | Sort | Create | Edit | Delete | Export |
|------|--------|--------|------------|------|--------|------|--------|--------|
| Brands | ✓ | - | ✓ | ✓ | ✓ | ✓ | ✓ | ○ |
| Perfumes | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ○ |
| Families | ✓ | - | ✓ | ✓ | ✓ | ✓ | ✓ | ○ |
| Notes | ✓ | - | ✓ | ✓ | ✓ | ✓ | ✓ | ○ |
| Accords | ✓ | - | ✓ | ✓ | ✓ | ✓ | ✓ | ○ |
| Tags | ✓ | - | ✓ | ✓ | ✓ | ✓ | ✓ | ○ |
| Seasons | ✓ | - | ✓ | ✓ | ✓ | ✓ | ✓ | ○ |
| Occasions | ✓ | - | ✓ | ✓ | ✓ | ✓ | ✓ | ○ |

✓ = Implemented | ○ = Optional/Phase 2

---

## Implementation Timeline

```
WEEK 1-2: Setup
├─ Project setup
├─ Build tools (Vite)
├─ Routing (React Router)
├─ State management (Zustand)
└─ API client setup

WEEK 2-3: Common Components
├─ Layout & Sidebar
├─ Data Table
├─ Forms framework
├─ Styling system
└─ Component library

WEEK 3-4: Simple Pages
├─ Dashboard
├─ Tags, Seasons, Occasions
└─ Basic CRUD patterns

WEEK 4-6: Catalog
├─ Brands management
├─ Families management
├─ Notes management
└─ Accords management

WEEK 6-8: Complex Features
├─ Perfumes (with relations)
├─ Expert System
└─ Advanced filtering

WEEK 8-9: Finalization
├─ Testing & QA
├─ Performance optimization
├─ Documentation
└─ Security review

WEEK 9-10: Deployment
├─ Staging
├─ UAT
└─ Production
```

---

## Key Features by Page Type

### List Pages (8 pages)
- ✓ Search by name/description
- ✓ Pagination (10/25/50 items per page)
- ✓ Sort by columns
- ✓ Delete with confirmation
- ✓ Edit/View action buttons
- ✓ Create new button
- ○ Bulk actions (Phase 2)
- ○ Export to CSV (Phase 2)

### Form Pages (16 pages)
- ✓ Field validation (client & server)
- ✓ Pre-fill on edit
- ✓ Dirty form detection
- ✓ Loading state
- ✓ Success/error notifications
- ✓ Cancel button (go back)
- ○ Auto-save draft (Phase 2)
- ○ Multi-step wizard (Phase 2)

### Specialized Pages

**Dashboard**:
- KPI cards (numbers, trends)
- Charts (if needed)
- Recent activities
- Quick actions
- Auto-refresh (5 min)

**Expert System**:
- User profile inputs (8 fields)
- Real-time testing
- Result display (tags, recommendations)
- "View Full Recommendation" flow
- Quick presets

**LLM Config**:
- Current provider display
- Available providers list
- Switch provider modal
- Settings form per provider
- Connection test button

---

## Data Models & Relationships

```
Brand
  └─ Perfumes (1:many)

Perfume (Complex Entity)
  ├─ Brand (many:1)
  ├─ Family (many:1)
  ├─ TopNotes (many:many) → Note
  ├─ MiddleNotes (many:many) → Note
  ├─ BaseNotes (many:many) → Note
  ├─ Accords (many:many) → Accord
  ├─ Seasons (many:many) → Season
  ├─ Occasions (many:many) → Occasion
  ├─ Tags (many:many) → Tag
  └─ Reviews (1:many)

Family
  └─ Perfumes (1:many)

Note
  └─ Perfumes (many:many)

Accord
  └─ Perfumes (many:many)

Tag
  └─ Perfumes (many:many)

Season
  └─ Perfumes (many:many)
  └─ Occasions (many:many)

Occasion
  └─ Perfumes (many:many)
```

---

## Validation Rules

### Brands
- Name: required, max 100 chars
- Description: optional, max 500 chars
- Website: optional, valid URL
- Country: optional, max 50 chars

### Perfumes
- Name: required, max 200 chars
- Brand: required (dropdown)
- Family: required (dropdown)
- Price: optional, min 0
- Top Notes: optional (multi-select)
- Base Notes: optional (multi-select)

### Notes
- Name: required, unique, max 100 chars
- Type: required (top/middle/base)
- Intensity: optional, 1-10

### Forms
- Email: valid format
- URLs: valid format
- Numbers: appropriate range
- Required fields: must be filled
- Unique constraints: checked on server

---

## Error Handling

### Types of Errors
- **Network**: Offline, timeout
- **Validation**: Invalid input format
- **Server**: 500, 503 errors
- **Auth**: 401, 403 errors
- **Not Found**: 404 errors
- **Conflict**: 409 duplicate errors

### Error Display
- **Toast**: Quick temporary messages
- **Alert**: Critical/important messages
- **Inline**: Field-specific validation errors
- **Page**: Full error pages for critical issues

### Retry Strategy
- Automatic retry for network errors (3x)
- Manual retry button
- Exponential backoff
- User notification

---

## Security & Performance

### Security
- ✓ JWT authentication
- ✓ CSRF token in forms
- ✓ Input sanitization
- ✓ XSS prevention
- ✓ CORS configuration
- ✓ Rate limiting
- ✓ Secure headers

### Performance
- ✓ Code splitting (by route)
- ✓ Lazy loading
- ✓ Request caching (React Query)
- ✓ Image optimization
- ✓ Pagination (not loading all)
- ✓ Debouncing (search)
- ○ Service worker (Phase 2)

### Accessibility
- ✓ Semantic HTML
- ✓ ARIA labels
- ✓ Keyboard navigation
- ✓ Color contrast
- ✓ Focus management
- WCAG 2.1 AA target

---

## Testing Coverage

### Unit Tests
- Components (rendering, props)
- Hooks (logic, state)
- Utilities (formatters, validators)
- API client

### Integration Tests
- Page workflows
- Form submission
- API mocking
- State management

### E2E Tests
- Complete user journeys
- Cross-browser
- Responsive design
- Performance

### Target Coverage
- ✓ >80% code coverage
- ✓ All pages tested
- ✓ All user flows tested
- ✓ All error scenarios tested

---

## Success Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| Page Load Time | <2 sec | Lighthouse |
| Feature Parity | 95% | Feature checklist |
| Code Coverage | >80% | Jest/Vitest report |
| Accessibility | WCAG 2.1 AA | Axe DevTools |
| Uptime | 99.9% | Monitoring |
| User Adoption | 100% in 2 weeks | Analytics |

---

## Deliverables

### Code
- [ ] React SPA application (all pages)
- [ ] API client services
- [ ] Component library
- [ ] Unit & integration tests
- [ ] E2E test suite

### Documentation
- [ ] Developer setup guide
- [ ] Component documentation
- [ ] API documentation
- [ ] User guide
- [ ] Deployment guide
- [ ] Architecture docs

### QA & Deployment
- [ ] Test plan & results
- [ ] Performance report
- [ ] Security audit
- [ ] Staging deployment
- [ ] Production deployment guide

---

## Rollout Strategy

### Phase 1: Internal Testing
- Deploy to staging
- Admin team tests
- Feedback collection
- Bug fixes

### Phase 2: Soft Launch
- Deploy to production
- Opt-in for users
- Monitor closely
- Support readiness

### Phase 3: Full Rollout
- Make default
- Migrate remaining users
- Monitor metrics
- Ongoing support

### Rollback Plan
- Keep MVC available for 2 weeks
- Easy switch back if critical issues
- Data sync between systems

---

## Questions & Next Steps

### Questions to Answer
1. React or Vue preference?
2. UI component library (Material-UI, Chakra, custom)?
3. Hosting platform (same as current)?
4. Database changes needed?
5. Offline support required?

### Next Steps
1. ✓ PRD approved
2. Set up development environment
3. Create detailed technical specs
4. Design API contracts
5. Begin Phase 1 (setup)
6. Start Phase 2 in parallel

---

**Document**: Admin Dashboard SPA Migration - Quick Reference  
**Version**: 1.0  
**Date**: March 19, 2026  
**For**: Full details, see ADMIN_SPA_MIGRATION_PRD.md
