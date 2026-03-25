# Admin Dashboard API Specification

**Version**: 1.0  
**Date**: March 19, 2026  
**Base URL**: `/api/v1/admin`  

---

## Authentication & Authorization

### Headers Required
```
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

### Response Codes
- `200`: Success
- `201`: Created
- `400`: Bad Request
- `401`: Unauthorized
- `403`: Forbidden
- `404`: Not Found
- `409`: Conflict (duplicate)
- `422`: Validation Error
- `500`: Server Error

---

## Error Response Format

```json
{
  "status": 400,
  "message": "Validation failed",
  "errors": [
    {
      "field": "name",
      "message": "Name is required"
    }
  ]
}
```

---

## Dashboard API

### Get Dashboard Statistics

```
GET /dashboard/stats
```

**Response**:
```json
{
  "totalBrands": 250,
  "totalPerfumes": 5000,
  "totalUsers": 15000,
  "totalReviews": 45000,
  "averageRating": 4.2,
  "recentActivity": [
    {
      "id": "1",
      "type": "perfume_added",
      "message": "New perfume added: Dior Sauvage",
      "timestamp": "2026-03-19T10:30:00Z"
    }
  ],
  "systemHealth": {
    "database": "ok",
    "cache": "ok",
    "llm": "ok"
  }
}
```

---

## Catalog APIs

### Standard Pagination Parameters

All list endpoints support:
- `page` (int, default: 1)
- `pageSize` (int, default: 10, max: 100)
- `search` (string, optional)
- `sortBy` (string, optional)
- `sortOrder` (asc/desc, default: asc)

### Standard List Response

```json
{
  "data": [
    { /* item */ }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalPages": 25,
    "totalCount": 250,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

---

## Brands API

### List Brands

```
GET /brands?page=1&pageSize=10&search=&sortBy=name&sortOrder=asc
```

**Response**:
```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Dior",
      "description": "Luxurious French fragrance house",
      "logoUrl": "https://example.com/logo.jpg",
      "website": "https://dior.com",
      "country": "France",
      "foundedYear": 1947,
      "perfumeCount": 45,
      "createdAt": "2026-01-15T10:00:00Z",
      "updatedAt": "2026-03-19T10:00:00Z"
    }
  ],
  "pagination": { /* ... */ }
}
```

### Get Brand

```
GET /brands/{id}
```

**Response**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Dior",
  "description": "Luxurious French fragrance house",
  "logoUrl": "https://example.com/logo.jpg",
  "website": "https://dior.com",
  "country": "France",
  "foundedYear": 1947,
  "perfumeCount": 45,
  "createdAt": "2026-01-15T10:00:00Z",
  "updatedAt": "2026-03-19T10:00:00Z"
}
```

### Create Brand

```
POST /brands
Content-Type: application/json

{
  "name": "Dior",
  "description": "Luxurious French fragrance house",
  "logoUrl": "https://example.com/logo.jpg",
  "website": "https://dior.com",
  "country": "France",
  "foundedYear": 1947
}
```

**Response**: `201 Created`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Dior",
  "description": "Luxurious French fragrance house",
  "logoUrl": "https://example.com/logo.jpg",
  "website": "https://dior.com",
  "country": "France",
  "foundedYear": 1947,
  "perfumeCount": 0,
  "createdAt": "2026-03-19T10:00:00Z",
  "updatedAt": "2026-03-19T10:00:00Z"
}
```

### Update Brand

```
PUT /brands/{id}
Content-Type: application/json

{
  "name": "Dior",
  "description": "Updated description",
  "logoUrl": "https://example.com/new-logo.jpg",
  "website": "https://dior.com",
  "country": "France",
  "foundedYear": 1947
}
```

**Response**: `200 OK`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Dior",
  "description": "Updated description",
  "logoUrl": "https://example.com/new-logo.jpg",
  "website": "https://dior.com",
  "country": "France",
  "foundedYear": 1947,
  "perfumeCount": 45,
  "createdAt": "2026-01-15T10:00:00Z",
  "updatedAt": "2026-03-19T10:30:00Z"
}
```

### Delete Brand

```
DELETE /brands/{id}
```

**Response**: `204 No Content`

**Note**: Returns `409 Conflict` if brand has associated perfumes.

---

## Perfumes API

### List Perfumes

```
GET /perfumes?page=1&pageSize=10&search=&brand=&family=&season=&sortBy=name&sortOrder=asc
```

**Response**:
```json
{
  "data": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440000",
      "name": "Sauvage",
      "description": "A fresh, spicy, and sophisticated fragrance",
      "brand": {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "name": "Dior"
      },
      "family": {
        "id": "770e8400-e29b-41d4-a716-446655440000",
        "name": "Aromatic"
      },
      "price": 89.99,
      "launchYear": 2015,
      "imageUrl": "https://example.com/sauvage.jpg",
      "topNotes": ["Bergamot", "Ambroxan"],
      "middleNotes": ["Pepper"],
      "baseNotes": ["Ambroxan"],
      "sillage": "strong",
      "longevity": "good",
      "seasons": ["Spring", "Summer"],
      "occasions": ["Office", "Casual"],
      "tags": ["Fresh", "Spicy", "Popular"],
      "rating": 4.5,
      "reviewCount": 1200,
      "createdAt": "2026-01-15T10:00:00Z",
      "updatedAt": "2026-03-19T10:00:00Z"
    }
  ],
  "pagination": { /* ... */ }
}
```

### Get Perfume

```
GET /perfumes/{id}
```

**Response**:
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440000",
  "name": "Sauvage",
  "description": "A fresh, spicy, and sophisticated fragrance",
  "brand": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Dior"
  },
  "family": {
    "id": "770e8400-e29b-41d4-a716-446655440000",
    "name": "Aromatic"
  },
  "price": 89.99,
  "launchYear": 2015,
  "imageUrl": "https://example.com/sauvage.jpg",
  "topNotes": ["Bergamot", "Ambroxan"],
  "middleNotes": ["Pepper"],
  "baseNotes": ["Ambroxan"],
  "accords": ["Spicy", "Fresh"],
  "sillage": "strong",
  "longevity": "good",
  "seasons": ["Spring", "Summer"],
  "occasions": ["Office", "Casual"],
  "tags": ["Fresh", "Spicy", "Popular"],
  "rating": 4.5,
  "reviewCount": 1200,
  "createdAt": "2026-01-15T10:00:00Z",
  "updatedAt": "2026-03-19T10:00:00Z"
}
```

### Get Perfume Reviews

```
GET /perfumes/{id}/reviews?page=1&pageSize=10
```

**Response**:
```json
{
  "data": [
    {
      "id": "880e8400-e29b-41d4-a716-446655440000",
      "username": "john_doe",
      "rating": 5,
      "title": "Fantastic fragrance!",
      "content": "Excellent quality and long lasting",
      "createdAt": "2026-03-19T10:00:00Z"
    }
  ],
  "pagination": { /* ... */ }
}
```

### Create Perfume

```
POST /perfumes
Content-Type: application/json

{
  "name": "Sauvage",
  "description": "A fresh, spicy, and sophisticated fragrance",
  "brandId": "550e8400-e29b-41d4-a716-446655440000",
  "familyId": "770e8400-e29b-41d4-a716-446655440000",
  "price": 89.99,
  "launchYear": 2015,
  "imageUrl": "https://example.com/sauvage.jpg",
  "topNoteIds": ["note-id-1", "note-id-2"],
  "middleNoteIds": ["note-id-3"],
  "baseNoteIds": ["note-id-4"],
  "accordIds": ["accord-id-1", "accord-id-2"],
  "sillage": "strong",
  "longevity": "good",
  "seasonIds": ["season-id-1", "season-id-2"],
  "occasionIds": ["occasion-id-1", "occasion-id-2"],
  "tagIds": ["tag-id-1", "tag-id-2"]
}
```

**Response**: `201 Created`
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440000",
  "name": "Sauvage",
  "description": "A fresh, spicy, and sophisticated fragrance",
  "brand": { /* ... */ },
  "family": { /* ... */ },
  "price": 89.99,
  "launchYear": 2015,
  "imageUrl": "https://example.com/sauvage.jpg",
  "topNotes": ["Bergamot", "Ambroxan"],
  "middleNotes": ["Pepper"],
  "baseNotes": ["Ambroxan"],
  "sillage": "strong",
  "longevity": "good",
  "seasons": ["Spring", "Summer"],
  "occasions": ["Office", "Casual"],
  "tags": ["Fresh", "Spicy", "Popular"],
  "rating": 0,
  "reviewCount": 0,
  "createdAt": "2026-03-19T10:00:00Z",
  "updatedAt": "2026-03-19T10:00:00Z"
}
```

### Update Perfume

```
PUT /perfumes/{id}
Content-Type: application/json

{
  /* same as create payload */
}
```

### Delete Perfume

```
DELETE /perfumes/{id}
```

---

## Families API

### List Families

```
GET /families?page=1&pageSize=10&search=&sortBy=name&sortOrder=asc
```

**Response**:
```json
{
  "data": [
    {
      "id": "770e8400-e29b-41d4-a716-446655440000",
      "name": "Aromatic",
      "description": "Fresh, herb-forward fragrances",
      "characteristics": "Herbal, minty, green",
      "perfumeCount": 234,
      "createdAt": "2026-01-15T10:00:00Z",
      "updatedAt": "2026-03-19T10:00:00Z"
    }
  ],
  "pagination": { /* ... */ }
}
```

### Get Family
```
GET /families/{id}
```

### Create Family
```
POST /families
Content-Type: application/json

{
  "name": "Aromatic",
  "description": "Fresh, herb-forward fragrances",
  "characteristics": "Herbal, minty, green"
}
```

### Update Family
```
PUT /families/{id}
```

### Delete Family
```
DELETE /families/{id}
```

---

## Notes API

### List Notes

```
GET /notes?page=1&pageSize=10&search=&type=&sortBy=name&sortOrder=asc
```

**Query Parameters**:
- `type`: `top`, `middle`, `base`, or empty for all

**Response**:
```json
{
  "data": [
    {
      "id": "990e8400-e29b-41d4-a716-446655440000",
      "name": "Bergamot",
      "description": "Citrus note with bright character",
      "type": "top",
      "category": "citrus",
      "intensity": 8,
      "color": "#FFB84D",
      "synonyms": ["Orange peel", "Citrus bergamia"],
      "perfumeCount": 567,
      "createdAt": "2026-01-15T10:00:00Z",
      "updatedAt": "2026-03-19T10:00:00Z"
    }
  ],
  "pagination": { /* ... */ }
}
```

### Get Note
```
GET /notes/{id}
```

### Create Note
```
POST /notes
Content-Type: application/json

{
  "name": "Bergamot",
  "description": "Citrus note with bright character",
  "type": "top",
  "category": "citrus",
  "intensity": 8,
  "color": "#FFB84D",
  "synonyms": ["Orange peel", "Citrus bergamia"]
}
```

### Update Note
```
PUT /notes/{id}
```

### Delete Note
```
DELETE /notes/{id}
```

---

## Accords API

### List Accords

```
GET /accords?page=1&pageSize=10&search=&sortBy=name&sortOrder=asc
```

**Response**:
```json
{
  "data": [
    {
      "id": "aa0e8400-e29b-41d4-a716-446655440000",
      "name": "Fresh Spicy",
      "description": "Combination of fresh and spicy notes",
      "compositionNotes": ["Bergamot", "Pepper", "Ambroxan"],
      "effect": "Creates a lively, energetic impression",
      "popularity": 9,
      "perfumeCount": 456,
      "createdAt": "2026-01-15T10:00:00Z",
      "updatedAt": "2026-03-19T10:00:00Z"
    }
  ],
  "pagination": { /* ... */ }
}
```

### Get Accord
```
GET /accords/{id}
```

### Create Accord
```
POST /accords
Content-Type: application/json

{
  "name": "Fresh Spicy",
  "description": "Combination of fresh and spicy notes",
  "compositionNoteIds": ["note-id-1", "note-id-2"],
  "effect": "Creates a lively, energetic impression",
  "popularity": 9
}
```

### Update Accord
```
PUT /accords/{id}
```

### Delete Accord
```
DELETE /accords/{id}
```

---

## Tags API

### List Tags

```
GET /tags?page=1&pageSize=10&search=&sortBy=name&sortOrder=asc
```

**Response**:
```json
{
  "data": [
    {
      "id": "bb0e8400-e29b-41d4-a716-446655440000",
      "name": "Fresh",
      "description": "Light, airy fragrances",
      "color": "#87CEEB",
      "category": "characteristic",
      "perfumeCount": 1234,
      "createdAt": "2026-01-15T10:00:00Z",
      "updatedAt": "2026-03-19T10:00:00Z"
    }
  ],
  "pagination": { /* ... */ }
}
```

### Get Tag
```
GET /tags/{id}
```

### Create Tag
```
POST /tags
Content-Type: application/json

{
  "name": "Fresh",
  "description": "Light, airy fragrances",
  "color": "#87CEEB",
  "category": "characteristic"
}
```

### Update Tag
```
PUT /tags/{id}
```

### Delete Tag
```
DELETE /tags/{id}
```

---

## Seasons API

### List Seasons

```
GET /seasons?page=1&pageSize=10&search=&sortBy=name&sortOrder=asc
```

**Response**:
```json
{
  "data": [
    {
      "id": "cc0e8400-e29b-41d4-a716-446655440000",
      "name": "Summer",
      "description": "Warm season fragrances",
      "recommendedNotes": ["Bergamot", "Citrus", "Aquatic"],
      "colorTheme": "#FFD700",
      "temperatureRange": "25-35°C",
      "perfumeCount": 890,
      "createdAt": "2026-01-15T10:00:00Z",
      "updatedAt": "2026-03-19T10:00:00Z"
    }
  ],
  "pagination": { /* ... */ }
}
```

### Get Season
```
GET /seasons/{id}
```

### Create Season
```
POST /seasons
Content-Type: application/json

{
  "name": "Summer",
  "description": "Warm season fragrances",
  "recommendedNoteIds": ["note-id-1", "note-id-2"],
  "colorTheme": "#FFD700",
  "temperatureRange": "25-35°C"
}
```

### Update Season
```
PUT /seasons/{id}
```

### Delete Season
```
DELETE /seasons/{id}
```

---

## Occasions API

### List Occasions

```
GET /occasions?page=1&pageSize=10&search=&sortBy=name&sortOrder=asc
```

**Response**:
```json
{
  "data": [
    {
      "id": "dd0e8400-e29b-41d4-a716-446655440000",
      "name": "Office",
      "description": "Professional workplace setting",
      "recommendedSillage": "moderate",
      "recommendedLongevity": "good",
      "timeOfDay": ["morning", "afternoon"],
      "genders": ["male", "female", "unisex"],
      "perfumeCount": 567,
      "createdAt": "2026-01-15T10:00:00Z",
      "updatedAt": "2026-03-19T10:00:00Z"
    }
  ],
  "pagination": { /* ... */ }
}
```

### Get Occasion
```
GET /occasions/{id}
```

### Create Occasion
```
POST /occasions
Content-Type: application/json

{
  "name": "Office",
  "description": "Professional workplace setting",
  "recommendedSillage": "moderate",
  "recommendedLongevity": "good",
  "timeOfDay": ["morning", "afternoon"],
  "genders": ["male", "female", "unisex"]
}
```

### Update Occasion
```
PUT /occasions/{id}
```

### Delete Occasion
```
DELETE /occasions/{id}
```

---

## Expert System API

### Test Expert System

```
POST /expert-system/test
Content-Type: application/json

{
  "climate": "hot",
  "occasion": "office",
  "skinType": "dry",
  "compliment": "high",
  "seasonPreference": "summer",
  "persona": "corporate",
  "sensitivity": "sensitive",
  "wantsLongPerformance": true
}
```

**Response**:
```json
{
  "prefer": ["woody", "aromatic", "musk"],
  "avoid": ["volatile_top_notes_only", "oriental_heavy"],
  "sillage": "moderate",
  "longevity": ">= medium",
  "reasons": [
    "Corporate persona → woody notes preferred",
    "Dry skin → stronger fragrances recommended"
  ],
  "result": "Based on your profile, we recommend..."
}
```

### Get Expert System Config

```
GET /expert-system/config
```

### Update Expert System Config

```
PUT /expert-system/config
Content-Type: application/json

{
  /* configuration data */
}
```

---

## LLM Configuration API

### List LLM Providers

```
GET /llm-providers
```

**Response**:
```json
{
  "providers": [
    {
      "id": "openai",
      "name": "OpenAI",
      "displayName": "OpenAI GPT-4",
      "status": "active",
      "description": "Advanced language model",
      "models": ["gpt-4", "gpt-3.5-turbo"],
      "configured": true
    },
    {
      "id": "claude",
      "name": "Claude",
      "displayName": "Anthropic Claude",
      "status": "inactive",
      "description": "Constitutional AI model",
      "models": ["claude-3-opus", "claude-3-sonnet"],
      "configured": false
    }
  ]
}
```

### Get Current Provider

```
GET /llm-providers/current
```

**Response**:
```json
{
  "id": "openai",
  "name": "OpenAI",
  "displayName": "OpenAI GPT-4",
  "status": "active",
  "model": "gpt-4",
  "temperature": 0.7,
  "maxTokens": 2000,
  "lastUsed": "2026-03-19T10:00:00Z"
}
```

### Switch Provider

```
POST /llm-providers/switch
Content-Type: application/json

{
  "providerId": "claude"
}
```

**Response**:
```json
{
  "success": true,
  "message": "Successfully switched to Claude",
  "provider": {
    "id": "claude",
    "name": "Claude",
    "status": "active"
  }
}
```

### Get Provider Configuration

```
GET /llm-providers/{providerId}/config
```

**Response**:
```json
{
  "id": "openai",
  "apiKey": "sk-***",
  "model": "gpt-4",
  "temperature": 0.7,
  "maxTokens": 2000,
  "systemPrompt": "You are an expert fragrance advisor",
  "costPerRequest": 0.03
}
```

### Update Provider Configuration

```
PUT /llm-providers/{providerId}/config
Content-Type: application/json

{
  "apiKey": "sk-new-key",
  "model": "gpt-4-turbo",
  "temperature": 0.8,
  "maxTokens": 3000,
  "systemPrompt": "Updated prompt"
}
```

**Response**: `200 OK`

### Test Provider Connection

```
POST /llm-providers/{providerId}/test
```

**Response**:
```json
{
  "success": true,
  "message": "Connection successful",
  "latency": "245ms",
  "timestamp": "2026-03-19T10:00:00Z"
}
```

---

## Validation Rules

### Brands
- `name`: Required, max 100 chars, unique
- `description`: Optional, max 500 chars
- `website`: Optional, valid URL format
- `country`: Optional, max 50 chars
- `foundedYear`: Optional, valid year (1800-2100)

### Perfumes
- `name`: Required, max 200 chars, unique per brand
- `brandId`: Required, must exist
- `familyId`: Required, must exist
- `price`: Optional, min 0, max 9999.99
- `launchYear`: Optional, valid year
- `topNoteIds`: Optional, array of existing note IDs
- `sillage`: Enum: weak, moderate, strong, very strong
- `longevity`: Enum: poor, moderate, good, excellent, eternal

### Notes
- `name`: Required, max 100 chars, unique
- `type`: Required, enum: top, middle, base
- `intensity`: Optional, 1-10
- `color`: Optional, hex color code format

### Accords
- `name`: Required, max 100 chars, unique
- `compositionNoteIds`: Required, min 2 notes, max 10
- `popularity`: Optional, 1-10

### Tags
- `name`: Required, max 50 chars, unique
- `color`: Optional, hex color code format
- `category`: Optional, max 50 chars

### Seasons
- `name`: Required, max 50 chars, unique
- `temperatureRange`: Optional, format: "20-30°C"

### Occasions
- `name`: Required, max 100 chars, unique
- `recommendedSillage`: Optional, enum values
- `recommendedLongevity`: Optional, enum values

---

## Rate Limiting

- **Limit**: 100 requests per minute per user
- **Headers**: 
  - `X-RateLimit-Limit: 100`
  - `X-RateLimit-Remaining: 95`
  - `X-RateLimit-Reset: 1234567890`

---

## Pagination Standards

- Minimum page size: 1
- Maximum page size: 100
- Default page size: 10
- First page: 1 (not 0)

---

## Sorting Standards

- `sortBy`: Field name (snake_case)
- `sortOrder`: `asc` or `desc`
- Default: By creation date, descending

---

## Caching Strategy

- GET requests: 5 minutes cache (if applicable)
- POST/PUT/DELETE: No cache (invalidate related)
- Cache-Control headers: `public, max-age=300`

---

## API Versioning

- Current version: `v1`
- Future: `/api/v2/admin/...`
- Backward compatibility guaranteed within major version

---

**API Specification Document**  
**Version**: 1.0  
**Date**: March 19, 2026
