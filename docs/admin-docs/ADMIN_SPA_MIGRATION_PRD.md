# PRD: Admin Dashboard Migration to Single Page App (SPA)

**Document Version**: 1.0  
**Date**: March 19, 2026  
**Status**: PLANNING  
**Project**: ALOud Perfume Admin Dashboard SPA Migration  
**Tech Stack**: Vue.js, TypeScript, Axios/Fetch API

---

## Executive Summary

This PRD outlines the complete migration of the MVC-based Admin Dashboard to a modern Single Page Application (SPA) that consumes the existing REST APIs. The migration will maintain all current functionality while providing a superior user experience, improved maintainability, and better performance.

### Key Goals

1. **Feature Parity**: Implement all current MVC admin features in the SPA
2. **API-First Architecture**: Leverage existing and create new REST APIs
3. **Improved UX**: Modern, responsive, component-based interface
4. **Better Performance**: Client-side rendering, faster navigation
5. **Maintainability**: TypeScript, component-based architecture, better testing

### Scope

- Admin Dashboard (Statistics & KPIs)
- Catalog Management (Brands, Perfumes, Families, Notes, Accords, Tags, Seasons, Occasions)
- Expert System Configuration
- LLM Configuration & Provider Management

---

## Current State Analysis

### Existing MVC Admin Pages

| Page            | Route                        | Purpose                   | Components                 |
| --------------- | ---------------------------- | ------------------------- | -------------------------- |
| Dashboard       | `/Admin`                     | View KPIs and statistics  | Stats cards, charts        |
| Brands List     | `/Admin/Brands`              | Manage brands             | Table, search, pagination  |
| Brand Create    | `/Admin/CreateBrand`         | Create new brand          | Form                       |
| Brand Edit      | `/Admin/EditBrand/{id}`      | Edit brand details        | Form                       |
| Perfumes List   | `/Admin/Perfumes`            | Manage perfumes           | Table, filters, pagination |
| Perfume Create  | `/Admin/CreatePerfume`       | Create new perfume        | Form with relations        |
| Perfume Edit    | `/Admin/EditPerfume/{id}`    | Edit perfume details      | Form with relations        |
| Perfume Details | `/Admin/PerfumeDetails/{id}` | View perfume details      | Details view               |
| Families List   | `/Admin/Families`            | Manage fragrance families | Table, search              |
| Family Create   | `/Admin/CreateFamily`        | Create family             | Form                       |
| Family Edit     | `/Admin/EditFamily/{id}`     | Edit family               | Form                       |
| Notes List      | `/Admin/Notes`               | Manage olfactive notes    | Table, search              |
| Note Create     | `/Admin/CreateNote`          | Create note               | Form                       |
| Note Edit       | `/Admin/EditNote/{id}`       | Edit note                 | Form                       |
| Accords List    | `/Admin/Accords`             | Manage accords            | Table, search              |
| Accord Create   | `/Admin/CreateAccord`        | Create accord             | Form                       |
| Accord Edit     | `/Admin/EditAccord/{id}`     | Edit accord               | Form                       |
| Tags List       | `/Admin/Tags`                | Manage tags               | Table, search              |
| Tag Create      | `/Admin/CreateTag`           | Create tag                | Form                       |
| Tag Edit        | `/Admin/EditTag/{id}`        | Edit tag                  | Form                       |
| Seasons List    | `/Admin/Seasons`             | Manage seasons            | Table, search              |
| Season Create   | `/Admin/CreateSeason`        | Create season             | Form                       |
| Season Edit     | `/Admin/EditSeason/{id}`     | Edit season               | Form                       |
| Occasions List  | `/Admin/Occasions`           | Manage occasions          | Table, search              |
| Occasion Create | `/Admin/CreateOccasion`      | Create occasion           | Form                       |
| Occasion Edit   | `/Admin/EditOccasion/{id}`   | Edit occasion             | Form                       |
| Expert System   | `/Admin/ExpertSystem`        | Configure expert system   | Forms, API testing         |
| LLM Config      | `/Admin/LLMConfig`           | Manage LLM providers      | Cards, settings            |

**Total**: 29 unique pages/views

---

## SPA Architecture

### Technology Stack

```
Frontend:
- Vue 3+
- TypeScript
- Vite (build tool)
-  Vue Router
- Axios or Fetch API
- TanStack Query (React Query) for state management
- Zustand or Pinia for global state
- Tailwind CSS / Material-UI for styling
- React Hook Form / VeeValidate for forms
- Zod or Yup for validation

Development:
- ESLint
- Prettier
- Vitest for unit tests
- Playwright for E2E tests
```

### Project Structure

```
admin-spa/
├── public/
├── src/
│   ├── api/
│   │   ├── client.ts              # Axios instance with auth
│   │   ├── brands.ts              # Brand API calls
│   │   ├── perfumes.ts            # Perfume API calls
│   │   ├── families.ts            # Family API calls
│   │   ├── notes.ts               # Note API calls
│   │   ├── accords.ts             # Accord API calls
│   │   ├── tags.ts                # Tag API calls
│   │   ├── seasons.ts             # Season API calls
│   │   ├── occasions.ts           # Occasion API calls
│   │   ├── dashboard.ts           # Dashboard API calls
│   │   ├── expertSystem.ts        # Expert System API calls
│   │   └── llmConfig.ts           # LLM Config API calls
│   ├── components/
│   │   ├── common/
│   │   │   ├── Layout.tsx         # Main layout wrapper
│   │   │   ├── Sidebar.tsx        # Navigation sidebar
│   │   │   ├── Header.tsx         # Top header
│   │   │   ├── DataTable.tsx      # Reusable table component
│   │   │   ├── SearchBar.tsx      # Search component
│   │   │   ├── Pagination.tsx     # Pagination component
│   │   │   ├── Modal.tsx          # Modal component
│   │   │   ├── Toast.tsx          # Toast notifications
│   │   │   ├── LoadingSpinner.tsx # Loading indicator
│   │   │   └── FormField.tsx      # Reusable form field
│   │   ├── forms/
│   │   │   ├── BrandForm.tsx      # Brand create/edit form
│   │   │   ├── PerfumeForm.tsx    # Perfume create/edit form
│   │   │   ├── FamilyForm.tsx     # Family form
│   │   │   ├── NoteForm.tsx       # Note form
│   │   │   ├── AccordForm.tsx     # Accord form
│   │   │   ├── TagForm.tsx        # Tag form
│   │   │   ├── SeasonForm.tsx     # Season form
│   │   │   └── OccasionForm.tsx   # Occasion form
│   │   ├── dashboard/
│   │   │   ├── StatsCard.tsx      # Stats card component
│   │   │   ├── Chart.tsx          # Chart component
│   │   │   └── KPIDashboard.tsx   # Main dashboard
│   │   ├── expertSystem/
│   │   │   ├── ExpertForm.tsx     # Expert system form
│   │   │   └── RecommendationView.tsx # Recommendation display
│   │   └── llmConfig/
│   │       ├── ProviderCard.tsx   # LLM provider card
│   │       └── LLMSettings.tsx    # LLM settings
│   ├── pages/
│   │   ├── dashboard/
│   │   │   └── Dashboard.tsx      # Dashboard page
│   │   ├── catalog/
│   │   │   ├── Brands.tsx         # Brands list page
│   │   │   ├── BrandForm.tsx      # Brand create/edit page
│   │   │   ├── Perfumes.tsx       # Perfumes list page
│   │   │   ├── PerfumeForm.tsx    # Perfume create/edit page
│   │   │   ├── PerfumeDetails.tsx # Perfume details page
│   │   │   ├── Families.tsx       # Families list page
│   │   │   ├── FamilyForm.tsx     # Family form page
│   │   │   ├── Notes.tsx          # Notes list page
│   │   │   ├── NoteForm.tsx       # Note form page
│   │   │   ├── Accords.tsx        # Accords list page
│   │   │   ├── AccordForm.tsx     # Accord form page
│   │   │   ├── Tags.tsx           # Tags list page
│   │   │   ├── TagForm.tsx        # Tag form page
│   │   │   ├── Seasons.tsx        # Seasons list page
│   │   │   ├── SeasonForm.tsx     # Season form page
│   │   │   ├── Occasions.tsx      # Occasions list page
│   │   │   └── OccasionForm.tsx   # Occasion form page
│   │   ├── expertSystem/
│   │   │   └── ExpertSystem.tsx   # Expert system page
│   │   └── settings/
│   │       └── LLMConfig.tsx      # LLM config page
│   ├── hooks/
│   │   ├── useBrands.ts           # Brand data hooks
│   │   ├── usePerfumes.ts         # Perfume data hooks
│   │   ├── useFamilies.ts         # Family data hooks
│   │   ├── useNotes.ts            # Note data hooks
│   │   ├── useAccords.ts          # Accord data hooks
│   │   ├── useTags.ts             # Tag data hooks
│   │   ├── useSeasons.ts          # Season data hooks
│   │   ├── useOccasions.ts        # Occasion data hooks
│   │   ├── useDashboard.ts        # Dashboard data hooks
│   │   ├── useAuth.ts             # Authentication hook
│   │   ├── useToast.ts            # Toast notification hook
│   │   └── usePagination.ts       # Pagination hook
│   ├── stores/
│   │   ├── authStore.ts           # Auth state (Zustand/Pinia)
│   │   ├── uiStore.ts             # UI state
│   │   └── toastStore.ts          # Toast state
│   ├── types/
│   │   ├── api.ts                 # API response types
│   │   ├── models.ts              # Data model types
│   │   └── forms.ts               # Form types
│   ├── utils/
│   │   ├── formatters.ts          # Format utilities
│   │   ├── validators.ts          # Validation utilities
│   │   ├── apiErrorHandler.ts     # Error handling
│   │   └── httpClient.ts          # HTTP client setup
│   ├── styles/
│   │   ├── globals.css            # Global styles
│   │   └── variables.css          # CSS variables
│   ├── App.tsx                    # Root app component
│   └── main.tsx                   # Entry point

├── tests/
│   ├── unit/
│   ├── integration/
│   └── e2e/
├── vite.config.ts
├── tsconfig.json
├── package.json
└── README.md
```

---

## Page Specifications & API Endpoints

### 1. Dashboard Page

**Route**: `/admin/dashboard`  
**Purpose**: Display KPIs and statistics

**API Endpoints**:

- `GET /api/v1/admin/dashboard/stats` - Get dashboard statistics

**Components Used**:

- `Layout` - Main layout
- `StatsCard` - Display individual stats
- `Chart` - Display charts (if needed)
- `Header` - Page header
- `Sidebar` - Navigation

**Page Content**:

- Total Brands card
- Total Perfumes card
- Total Users card
- Recent Activities feed
- System Health status
- Charts/Graphs (if applicable)

**Features**:

- Auto-refresh every 5 minutes
- Responsive design
- Export stats (optional)

---

### 2. Brands Management

#### 2.1 Brands List Page

**Route**: `/admin/catalog/brands`  
**Purpose**: List and manage brands

**API Endpoints**:

- `GET /api/v1/admin/brands?page=1&pageSize=10&search=` - Get brands with pagination
- `DELETE /api/v1/admin/brands/{id}` - Delete brand

**Components Used**:

- `Layout`, `Header`, `Sidebar`
- `DataTable` - Display brands
- `SearchBar` - Search brands
- `Pagination` - Navigate pages
- `Modal` - Confirm delete
- `Toast` - Notifications

**Page Content**:

- Search bar (by name, description)
- Table with columns:
    - Brand Name
    - Description
    - Created Date
    - Actions (Edit, Delete, View)
- Pagination controls
- "Create New Brand" button
- Bulk actions (optional)

**Features**:

- Search functionality
- Pagination (10/25/50 items per page)
- Sort by columns
- Delete confirmation modal
- Export to CSV (optional)

---

#### 2.2 Brand Create Page

**Route**: `/admin/catalog/brands/create`  
**Purpose**: Create new brand

**API Endpoints**:

- `POST /api/v1/admin/brands` - Create new brand

**Components Used**:

- `Layout`, `Header`, `Sidebar`
- `BrandForm` - Form component
- `FormField` - Individual fields
- `Toast` - Success/error notifications

**Form Fields**:

- Brand Name (required, text)
- Description (optional, textarea)
- Logo URL (optional, text)
- Website (optional, URL)
- Country (optional, text)
- Founded Year (optional, number)

**Features**:

- Form validation (client & server)
- Image preview (if logo URL provided)
- Cancel button (goes back to list)
- Submit button
- Loading state during submission
- Success toast notification

---

#### 2.3 Brand Edit Page

**Route**: `/admin/catalog/brands/:id/edit`  
**Purpose**: Edit existing brand

**API Endpoints**:

- `GET /api/v1/admin/brands/{id}` - Get brand details
- `PUT /api/v1/admin/brands/{id}` - Update brand
- `DELETE /api/v1/admin/brands/{id}` - Delete brand

**Components Used**:

- `Layout`, `Header`, `Sidebar`
- `BrandForm` - Form component (pre-filled)
- `FormField` - Individual fields
- `Toast` - Notifications
- `Modal` - Delete confirmation

**Features**:

- Pre-fill form with current data
- Loading state while fetching
- Dirty form detection (warn on unsaved changes)
- Save button
- Delete button (with confirmation)
- Cancel button
- Success/error toast notifications

---

### 3. Perfumes Management

#### 3.1 Perfumes List Page

**Route**: `/admin/catalog/perfumes`  
**Purpose**: List and manage perfumes

**API Endpoints**:

- `GET /api/v1/admin/perfumes?page=1&pageSize=10&search=&brand=&family=` - Get perfumes
- `DELETE /api/v1/admin/perfumes/{id}` - Delete perfume

**Components Used**:

- `Layout`, `Header`, `Sidebar`
- `DataTable` - Display perfumes
- `SearchBar` - Search
- `FilterBar` - Filter by brand/family
- `Pagination` - Navigation
- `Modal` - Confirm delete
- `Toast` - Notifications

**Page Content**:

- Search bar
- Filter dropdowns (Brand, Family, Season, etc.)
- Table with columns:
    - Perfume Name
    - Brand
    - Family
    - Notes Preview
    - Rating
    - Created Date
    - Actions
- Pagination controls
- "Create New Perfume" button

**Features**:

- Multi-column search & filter
- Advanced filtering
- Pagination
- Sort by columns
- Bulk delete
- Export to CSV (optional)

---

#### 3.2 Perfume Create Page

**Route**: `/admin/catalog/perfumes/create`  
**Purpose**: Create new perfume

**API Endpoints**:

- `POST /api/v1/admin/perfumes` - Create perfume
- `GET /api/v1/admin/brands` - Get brands for dropdown
- `GET /api/v1/admin/families` - Get families for dropdown
- `GET /api/v1/admin/notes` - Get notes for dropdown
- `GET /api/v1/admin/seasons` - Get seasons for dropdown
- `GET /api/v1/admin/occasions` - Get occasions for dropdown

**Components Used**:

- `Layout`, `Header`, `Sidebar`
- `PerfumeForm` - Multi-section form
- `FormField` - Individual fields
- `Toast` - Notifications

**Form Sections**:

**Basic Info**:

- Perfume Name (required)
- Description (optional)
- Brand (required, dropdown)
- Retail Price (optional)
- Launch Year (optional)
- Image URL (optional)

**Olfactive Properties**:

- Top Notes (multi-select from Notes list)
- Middle Notes (multi-select)
- Base Notes (multi-select)
- Family (required, dropdown)
- Accords (multi-select)

**Characteristics**:

- Sillage (dropdown: weak/moderate/strong/very strong)
- Longevity (dropdown: poor/moderate/good/excellent/eternal)
- Seasons (multi-select)
- Occasions (multi-select)
- Genders (multi-select: male/female/unisex)

**Optional**:

- Tags (multi-select)
- Rating (1-10 slider)
- Notes/Comments (textarea)

**Features**:

- Step-by-step or scrollable form
- Live preview of selections
- Related items preview
- Form validation
- Auto-save draft (optional)
- Cancel button
- Submit button with loading state

---

#### 3.3 Perfume Edit Page

**Route**: `/admin/catalog/perfumes/:id/edit`  
**Purpose**: Edit existing perfume

**API Endpoints**:

- `GET /api/v1/admin/perfumes/{id}` - Get perfume details
- `PUT /api/v1/admin/perfumes/{id}` - Update perfume
- `DELETE /api/v1/admin/perfumes/{id}` - Delete perfume
- `GET /api/v1/admin/brands` - Get brands for dropdown
- `GET /api/v1/admin/families` - Get families for dropdown
- `GET /api/v1/admin/notes` - Get notes for dropdown
- `GET /api/v1/admin/seasons` - Get seasons for dropdown
- `GET /api/v1/admin/occasions` - Get occasions for dropdown

**Components Used**:

- Same as Create page
- Plus `Modal` for delete confirmation
- Plus loading skeleton while fetching

**Features**:

- Pre-fill form with current data
- Show related items (how many reviews, ratings)
- Dirty form detection
- Save button
- Delete button with confirmation
- Cancel button
- Change history (optional)
- Versioning (optional)

---

#### 3.4 Perfume Details Page

**Route**: `/admin/catalog/perfumes/:id`  
**Purpose**: View comprehensive perfume details

**API Endpoints**:

- `GET /api/v1/admin/perfumes/{id}` - Get perfume details
- `GET /api/v1/admin/perfumes/{id}/reviews` - Get reviews (optional)

**Components Used**:

- `Layout`, `Header`, `Sidebar`
- Stats cards
- Tabs (Details, Reviews, Relations)
- Modal - Edit inline

**Page Content**:

- Perfume image/cover
- Basic info (name, brand, price)
- Stats (rating, reviews count, wishlists)
- Detailed properties (tabs)
- Related perfumes
- Edit button
- Back button

---

### 4. Families Management

#### 4.1 Families List Page

**Route**: `/admin/catalog/families`  
**Purpose**: List and manage fragrance families

**API Endpoints**:

- `GET /api/v1/admin/families?page=1&pageSize=10&search=` - Get families
- `DELETE /api/v1/admin/families/{id}` - Delete family

**Components Used**:

- `Layout`, `Header`, `Sidebar`
- `DataTable` - List families
- `SearchBar` - Search
- `Pagination` - Navigate
- `Modal` - Delete confirmation
- `Toast` - Notifications

**Page Content**:

- Search bar
- Table with columns:
    - Family Name
    - Description
    - Perfumes Count
    - Created Date
    - Actions
- "Create New Family" button
- Pagination controls

---

#### 4.2 Family Create/Edit Pages

**Route**: `/admin/catalog/families/create` and `/admin/catalog/families/:id/edit`

**API Endpoints**:

- `POST /api/v1/admin/families` - Create
- `GET /api/v1/admin/families/{id}` - Get details
- `PUT /api/v1/admin/families/{id}` - Update
- `DELETE /api/v1/admin/families/{id}` - Delete

**Components Used**:

- `Layout`, `Header`, `Sidebar`
- `FamilyForm` - Form component
- `FormField` - Fields
- `Toast` - Notifications

**Form Fields**:

- Family Name (required, text)
- Description (optional, textarea)
- Characteristics (optional, textarea)
- Representative Perfumes (optional, multi-select)

---

### 5. Notes Management

#### 5.1 Notes List Page

**Route**: `/admin/catalog/notes`

**API Endpoints**:

- `GET /api/v1/admin/notes?page=1&pageSize=10&search=` - Get notes
- `DELETE /api/v1/admin/notes/{id}` - Delete note

**Similar Structure to Families**

---

#### 5.2 Note Create/Edit Pages

**Route**: `/admin/catalog/notes/create` and `/admin/catalog/notes/:id/edit`

**API Endpoints**:

- `POST /api/v1/admin/notes` - Create
- `GET /api/v1/admin/notes/{id}` - Get details
- `PUT /api/v1/admin/notes/{id}` - Update
- `DELETE /api/v1/admin/notes/{id}` - Delete

**Form Fields**:

- Note Name (required)
- Description (optional)
- Note Type (required, dropdown: top/middle/base)
- Category (optional, dropdown)
- Intensity (optional, 1-10 slider)
- Color (optional, color picker)
- Synonyms (optional, comma-separated)

---

### 6. Accords Management

#### 6.1 Accords List Page

**Route**: `/admin/catalog/accords`

**API Endpoints**:

- `GET /api/v1/admin/accords?page=1&pageSize=10&search=` - Get accords
- `DELETE /api/v1/admin/accords/{id}` - Delete accord

**Similar Structure**

---

#### 6.2 Accord Create/Edit Pages

**Route**: `/admin/catalog/accords/create` and `/admin/catalog/accords/:id/edit`

**API Endpoints**:

- `POST /api/v1/admin/accords` - Create
- `GET /api/v1/admin/accords/{id}` - Get details
- `PUT /api/v1/admin/accords/{id}` - Update
- `DELETE /api/v1/admin/accords/{id}` - Delete

**Form Fields**:

- Accord Name (required)
- Description (optional)
- Composition Notes (multi-select)
- Effect/Characteristic (textarea)
- Popularity (optional, 1-10 slider)

---

### 7. Tags Management

#### 7.1 Tags List Page

**Route**: `/admin/catalog/tags`

**API Endpoints**:

- `GET /api/v1/admin/tags?page=1&pageSize=10&search=` - Get tags
- `DELETE /api/v1/admin/tags/{id}` - Delete tag

---

#### 7.2 Tag Create/Edit Pages

**Route**: `/admin/catalog/tags/create` and `/admin/catalog/tags/:id/edit`

**API Endpoints**:

- `POST /api/v1/admin/tags` - Create
- `GET /api/v1/admin/tags/{id}` - Get details
- `PUT /api/v1/admin/tags/{id}` - Update
- `DELETE /api/v1/admin/tags/{id}` - Delete

**Form Fields**:

- Tag Name (required)
- Description (optional)
- Color (optional, color picker)
- Category (optional, dropdown)

---

### 8. Seasons Management

#### 8.1 Seasons List Page

**Route**: `/admin/catalog/seasons`

**API Endpoints**:

- `GET /api/v1/admin/seasons?page=1&pageSize=10&search=` - Get seasons
- `DELETE /api/v1/admin/seasons/{id}` - Delete season

---

#### 8.2 Season Create/Edit Pages

**Route**: `/admin/catalog/seasons/create` and `/admin/catalog/seasons/:id/edit`

**API Endpoints**:

- `POST /api/v1/admin/seasons` - Create
- `GET /api/v1/admin/seasons/{id}` - Get details
- `PUT /api/v1/admin/seasons/{id}` - Update
- `DELETE /api/v1/admin/seasons/{id}` - Delete

**Form Fields**:

- Season Name (required)
- Description (optional)
- Recommended Notes (multi-select)
- Color Theme (optional, color picker)
- Temperature Range (optional)

---

### 9. Occasions Management

#### 9.1 Occasions List Page

**Route**: `/admin/catalog/occasions`

**API Endpoints**:

- `GET /api/v1/admin/occasions?page=1&pageSize=10&search=` - Get occasions
- `DELETE /api/v1/admin/occasions/{id}` - Delete occasion

---

#### 9.2 Occasion Create/Edit Pages

**Route**: `/admin/catalog/occasions/create` and `/admin/catalog/occasions/:id/edit`

**API Endpoints**:

- `POST /api/v1/admin/occasions` - Create
- `GET /api/v1/admin/occasions/{id}` - Get details
- `PUT /api/v1/admin/occasions/{id}` - Update
- `DELETE /api/v1/admin/occasions/{id}` - Delete

**Form Fields**:

- Occasion Name (required)
- Description (optional)
- Recommended Sillage (dropdown)
- Recommended Longevity (dropdown)
- Time of Day (multi-select)
- Gender (multi-select)

---

### 10. Expert System Configuration Page

**Route**: `/admin/settings/expert-system`  
**Purpose**: Configure and test expert system

**API Endpoints**:

- `POST /api/v1/ai/expert-system/test` - Test recommendation
- `GET /api/v1/admin/expert-system/config` - Get current config (optional)
- `PUT /api/v1/admin/expert-system/config` - Update config (optional)

**Components Used**:

- `Layout`, `Header`, `Sidebar`
- `ExpertForm` - Complex form with multiple sections
- `FormField` - Individual fields
- `RecommendationView` - Display recommendations
- `Toast` - Notifications

**Page Content**:

**Configuration Section** (Collapsible):

- Rule Sets (optional)
- Weighting Algorithms (optional)
- Default Preferences (optional)

**Testing Section**:

- User Profile Dropdowns:
    - Climate (required)
    - Occasion (required)
    - Skin Type (required)
    - Compliment Level (required)
    - Season Preference (required)
    - Persona (required)
    - Sensitivity (required)
    - Wants Long Performance (checkbox)

**Results Section**:

- Preferred Notes (tags)
- Notes to Avoid (tags)
- Sillage Recommendation
- Longevity Recommendation
- Reasons (list)
- LLM-Generated Analysis
- "View Full Recommendation" button

**Features**:

- Real-time testing
- Quick presets (e.g., "Summer Casual", "Winter Formal")
- Save test results (optional)
- Export results
- Clear form button

---

### 11. LLM Configuration Page

**Route**: `/admin/settings/llm-config`  
**Purpose**: Manage LLM providers and configuration

**API Endpoints**:

- `GET /api/v1/admin/llm-providers` - Get available LLM providers
- `GET /api/v1/admin/llm-providers/current` - Get current provider
- `POST /api/v1/admin/llm-providers/switch` - Switch provider
- `PUT /api/v1/admin/llm-providers/{id}/config` - Update provider config

**Components Used**:

- `Layout`, `Header`, `Sidebar`
- `ProviderCard` - Display provider info
- `LLMSettings` - Provider settings form
- `Modal` - Confirm switch
- `Toast` - Notifications
- `Alert` - Important notices

**Page Content**:

**Current Provider Card**:

- Provider name
- Status (active/inactive)
- Configuration summary
- "Switch Provider" button
- "Configure" button

**Available Providers**:

- Cards for each provider (OpenAI, Claude, etc.)
- Provider details
- Status indicator
- "Switch To" button

**Configuration Form** (Modal):

- API Key (password field)
- Model Selection (dropdown)
- Temperature (slider)
- Max Tokens (number)
- Other provider-specific settings
- Save button
- Test button (optional)

**Features**:

- Provider comparison
- One-click switching
- Configuration per provider
- Test connection button
- Usage statistics (optional)
- Cost estimation (optional)

---

## API Endpoints Required

### Admin APIs (To be created/extended)

```
BASE_URL: /api/v1/admin

BRANDS:
GET    /brands?page=1&pageSize=10&search=
GET    /brands/{id}
POST   /brands
PUT    /brands/{id}
DELETE /brands/{id}

PERFUMES:
GET    /perfumes?page=1&pageSize=10&search=&brand=&family=
GET    /perfumes/{id}
GET    /perfumes/{id}/reviews
POST   /perfumes
PUT    /perfumes/{id}
DELETE /perfumes/{id}

FAMILIES:
GET    /families?page=1&pageSize=10&search=
GET    /families/{id}
POST   /families
PUT    /families/{id}
DELETE /families/{id}

NOTES:
GET    /notes?page=1&pageSize=10&search=
GET    /notes/{id}
POST   /notes
PUT    /notes/{id}
DELETE /notes/{id}

ACCORDS:
GET    /accords?page=1&pageSize=10&search=
GET    /accords/{id}
POST   /accords
PUT    /accords/{id}
DELETE /accords/{id}

TAGS:
GET    /tags?page=1&pageSize=10&search=
GET    /tags/{id}
POST   /tags
PUT    /tags/{id}
DELETE /tags/{id}

SEASONS:
GET    /seasons?page=1&pageSize=10&search=
GET    /seasons/{id}
POST   /seasons
PUT    /seasons/{id}
DELETE /seasons/{id}

OCCASIONS:
GET    /occasions?page=1&pageSize=10&search=
GET    /occasions/{id}
POST   /occasions
PUT    /occasions/{id}
DELETE /occasions/{id}

DASHBOARD:
GET    /dashboard/stats

EXPERT SYSTEM:
GET    /expert-system/config
PUT    /expert-system/config
POST   /expert-system/test

LLM CONFIG:
GET    /llm-providers
GET    /llm-providers/current
POST   /llm-providers/switch
PUT    /llm-providers/{id}/config
POST   /llm-providers/{id}/test
```

---

## Component Library

### Common Components

| Component        | Location             | Purpose             | Props                            |
| ---------------- | -------------------- | ------------------- | -------------------------------- |
| `Layout`         | `components/common/` | Main layout wrapper | children, sidebar                |
| `Sidebar`        | `components/common/` | Navigation sidebar  | items, active                    |
| `Header`         | `components/common/` | Top header bar      | title, actions                   |
| `DataTable`      | `components/common/` | Reusable table      | columns, data, onEdit, onDelete  |
| `SearchBar`      | `components/common/` | Search input        | onSearch, placeholder            |
| `Pagination`     | `components/common/` | Pagination controls | currentPage, total, onPageChange |
| `Modal`          | `components/common/` | Modal dialog        | title, content, actions, open    |
| `Toast`          | `components/common/` | Toast notifications | message, type, duration          |
| `LoadingSpinner` | `components/common/` | Loading indicator   | size, fullscreen                 |
| `FormField`      | `components/common/` | Form field wrapper  | label, type, value, onChange     |

### Form Components

| Component      | Location            | Purpose                  |
| -------------- | ------------------- | ------------------------ |
| `BrandForm`    | `components/forms/` | Brand create/edit form   |
| `PerfumeForm`  | `components/forms/` | Perfume create/edit form |
| `FamilyForm`   | `components/forms/` | Family form              |
| `NoteForm`     | `components/forms/` | Note form                |
| `AccordForm`   | `components/forms/` | Accord form              |
| `TagForm`      | `components/forms/` | Tag form                 |
| `SeasonForm`   | `components/forms/` | Season form              |
| `OccasionForm` | `components/forms/` | Occasion form            |

### Page Components

| Component        | Location              | Purpose              |
| ---------------- | --------------------- | -------------------- |
| `Dashboard`      | `pages/dashboard/`    | Dashboard page       |
| `BrandsList`     | `pages/catalog/`      | Brands list          |
| `PerfumesList`   | `pages/catalog/`      | Perfumes list        |
| `PerfumeDetails` | `pages/catalog/`      | Perfume details view |
| `ExpertSystem`   | `pages/expertSystem/` | Expert system config |
| `LLMConfig`      | `pages/settings/`     | LLM configuration    |

---

## Hooks (React) / Composables (Vue)

### Data Fetching Hooks

```typescript
// useBrands.ts
useBrands(pageIndex, pageSize, searchTerm);
useBrand(id);
useCreateBrand();
useUpdateBrand(id);
useDeleteBrand(id);

// Similar for:
// usePerfumes, useFamilies, useNotes, useAccords,
// useTags, useSeasons, useOccasions, useDashboard
```

### UI Hooks

```typescript
// useToast.ts
useToast(); // { success, error, info, warning }

// useAuth.ts
useAuth(); // { user, isAuthenticated, logout }

// usePagination.ts
usePagination(total, pageSize); // { currentPage, pageSize, totalPages }

// useForm.ts
useForm(initialValues, onSubmit, validate);
```

---

## State Management

### Global State (Zustand/Pinia)

```typescript
// authStore
- currentUser
- isAuthenticated
- login(credentials)
- logout()

// uiStore
- sidebarOpen
- theme
- toggleSidebar()
- toggleTheme()

// toastStore
- toasts[]
- addToast(message, type, duration)
- removeToast(id)
```

---

## Responsive Design

### Breakpoints

```css
xs: 0px
sm: 640px
md: 768px
lg: 1024px
xl: 1280px
2xl: 1536px
```

### Mobile Considerations

- Responsive sidebar (collapse/drawer)
- Touch-friendly buttons and inputs
- Mobile-optimized tables (card view on small screens)
- Responsive forms (single column on mobile)
- Swipe gestures for navigation

---

## Security Considerations

1. **Authentication**: JWT tokens in localStorage/sessionStorage
2. **Authorization**: Role-based access control on routes
3. **CSRF Protection**: Include CSRF token in POST/PUT/DELETE requests
4. **Input Validation**: Client-side and rely on server validation
5. **XSS Prevention**: Sanitize HTML, use React's built-in escaping
6. **CORS**: Configure appropriate CORS headers
7. **Rate Limiting**: Implement request throttling
8. **API Keys**: Never expose in client-side code

---

## Performance Considerations

1. **Code Splitting**: Lazy load page components
2. **Image Optimization**: Compress images, use WebP
3. **Caching**: Implement request caching with React Query
4. **Pagination**: Load data per page (not all at once)
5. **Debouncing**: Debounce search input
6. **Virtualization**: Virtual scrolling for large lists
7. **Service Workers**: Optional PWA for offline support

---

## Error Handling

### Error Types

1. **Network Errors**: Retry mechanism, offline handling
2. **Validation Errors**: Display field-specific errors
3. **Server Errors**: Display user-friendly error messages
4. **Auth Errors**: Redirect to login, refresh token
5. **Not Found**: Display 404 page or redirect

### Error Display

- Toast notifications for temporary errors
- Alert modals for critical errors
- Inline field errors for form validation
- Error boundary for component crashes

---

## Testing Strategy

### Unit Tests

- Component rendering
- Hook logic
- Utility functions
- API client functions

### Integration Tests

- Page workflows
- Form submission
- API integration
- State management

### E2E Tests

- Complete user journeys
- Cross-browser testing
- Performance testing
- Accessibility testing

---

## Migration Plan

### Phase 1: Setup & Infrastructure (Week 1-2)

- [ ] Set up React/Vue project
- [ ] Configure build tools (Vite)
- [ ] Set up routing
- [ ] Configure state management
- [ ] Create API client
- [ ] Set up authentication

### Phase 2: Common Components (Week 2-3)

- [ ] Build common components (Layout, Table, Form fields)
- [ ] Create reusable hooks
- [ ] Set up styling system
- [ ] Create component library

### Phase 3: Dashboard & Simple Pages (Week 3-4)

- [ ] Dashboard page
- [ ] Tags management (simplest)
- [ ] Seasons management
- [ ] Occasions management

### Phase 4: Catalog Management (Week 4-6)

- [ ] Brands management
- [ ] Families management
- [ ] Notes management
- [ ] Accords management

### Phase 5: Complex Pages (Week 6-8)

- [ ] Perfumes management
- [ ] Perfume details page
- [ ] Expert system configuration

### Phase 6: Settings & Finalization (Week 8-9)

- [ ] LLM configuration
- [ ] Settings page
- [ ] Testing & QA
- [ ] Performance optimization
- [ ] Documentation

### Phase 7: Deployment (Week 9-10)

- [ ] Staging deployment
- [ ] UAT testing
- [ ] Production deployment
- [ ] Monitor & support

---

## Success Metrics

1. **Performance**: Page load time < 2 seconds
2. **User Experience**: 95%+ feature parity with MVC
3. **Code Quality**: >80% test coverage
4. **Accessibility**: WCAG 2.1 AA compliance
5. **SEO**: Meta tags, proper semantic HTML
6. **Uptime**: 99.9% availability
7. **User Adoption**: 100% of admin users on SPA within 2 weeks

---

## Dependencies & Tech Versions

```json
{
    "react": "^18.3.0",
    "react-dom": "^18.3.0",
    "react-router-dom": "^6.22.0",
    "axios": "^1.6.5",
    "zustand": "^4.4.0",
    "react-hook-form": "^7.48.0",
    "zod": "^3.22.4",
    "vite": "^5.0.0",
    "typescript": "^5.3.0",
    "tailwindcss": "^3.4.1",
    "@tanstack/react-query": "^5.28.0",
    "vitest": "^1.0.0",
    "playwright": "^1.40.0"
}
```

---

## Documentation Requirements

1. **Component Documentation**: Storybook setup
2. **API Documentation**: Swagger/OpenAPI specs
3. **Developer Guide**: Setup, development, testing
4. **User Guide**: Admin dashboard functionality
5. **Deployment Guide**: Production deployment steps

---

## Risks & Mitigation

| Risk                       | Impact | Mitigation                               |
| -------------------------- | ------ | ---------------------------------------- |
| Data Loss During Migration | High   | Backup all data, run parallel systems    |
| User Adoption Issues       | High   | Training, gradual rollout, support       |
| Performance Issues         | Medium | Load testing, caching, optimization      |
| API Changes Breaking       | Medium | Version APIs, backward compatibility     |
| Security Vulnerabilities   | High   | Security audit, pen testing, code review |
| Timeline Slippage          | Medium | Buffer time, agile approach, prioritize  |

---

## Sign-Off

- **Product Owner**: ********\_********
- **Tech Lead**: ********\_********
- **Project Manager**: ********\_********
- **Date**: ********\_********

---

**Document Version History**:

- v1.0 - Initial PRD (March 19, 2026)

**Next Steps**:

1. Review and approve PRD
2. Set up development environment
3. Create detailed technical specifications
4. Begin Phase 1 implementation
