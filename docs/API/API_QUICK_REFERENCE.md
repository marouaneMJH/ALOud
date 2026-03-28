# ALOud REST API - Quick Reference

Quick lookup guide for all 62 REST API endpoints organized by resource type.

---

## Admin Endpoints (4)

| # | Method | Endpoint | Purpose | Auth |
|----|--------|----------|---------|------|
| 1 | GET | `/admin/dashboard` | Get dashboard statistics | 🔒 |
| 2 | GET | `/admin/llm-config` | Get LLM providers | 🔒 |
| 3 | POST | `/admin/llm-config/switch-provider` | Switch LLM provider | 🔒 |
| 4 | GET | `/admin/expert-system` | Get expert system config | 🔒 |

---

## Health Endpoints (1)

| # | Method | Endpoint | Purpose | Auth |
|----|--------|----------|---------|------|
| 5 | GET | `/health` | Check service health | 🌐 |

---

## Account Endpoints (5)

| # | Method | Endpoint | Purpose | Auth |
|----|--------|----------|---------|------|
| 6 | POST | `/accounts/register` | Register new user | 🌐 |
| 7 | POST | `/accounts/login` | User login | 🌐 |
| 8 | POST | `/accounts/verify` | Verify email | 🌐 |
| 9 | POST | `/accounts/resend-verification` | Resend verification code | 🌐 |
| 10 | GET | `/accounts/profile` | Get user profile | 🔒 |

---

## Perfume Endpoints (10)

### Admin
| # | Method | Endpoint | Purpose | Auth |
|----|--------|----------|---------|------|
| 11 | GET | `/perfumes/admin` | List perfumes (admin) | 🔒 |
| 12 | GET | `/perfumes/admin/select` | Get perfume select list | 🔒 |
| 13 | POST | `/perfumes/admin` | Create perfume | 🔒 |
| 14 | GET | `/perfumes/admin/{id}` | Get perfume for edit | 🔒 |
| 15 | PUT | `/perfumes/admin/{id}` | Update perfume | 🔒 |
| 16 | DELETE | `/perfumes/admin/{id}` | Delete perfume | 🔒 |
| 17 | POST | `/perfumes/admin/validate-exists` | Validate perfume name | 🔒 |

### Public
| # | Method | Endpoint | Purpose | Auth |
|----|--------|----------|---------|------|
| 18 | GET | `/perfumes` | Get perfumes (customer) | 🌐 |
| 19 | GET | `/perfumes/{id}` | Get perfume details | 🌐 |
| 20 | POST | `/perfumes/add-to-cart` | Add to cart | 🌐 |

---

## Brands Endpoints (7)

| # | Method | Endpoint | Purpose | Auth |
|----|--------|----------|---------|------|
| 21 | GET | `/admin/brands` | List brands | 🔒 |
| 22 | GET | `/admin/brands/select` | Get brands for select | 🔒 |
| 23 | POST | `/admin/brands` | Create brand | 🔒 |
| 24 | GET | `/admin/brands/{id}` | Get brand for edit | 🔒 |
| 25 | PUT | `/admin/brands/{id}` | Update brand | 🔒 |
| 26 | DELETE | `/admin/brands/{id}` | Delete brand | 🔒 |
| 27 | POST | `/admin/brands/validate-exists` | Validate brand name | 🔒 |

---

## Families Endpoints (7)

| # | Method | Endpoint | Purpose | Auth |
|----|--------|----------|---------|------|
| 28 | GET | `/admin/families` | List families | 🔒 |
| 29 | GET | `/admin/families/select` | Get families for select | 🔒 |
| 30 | POST | `/admin/families` | Create family | 🔒 |
| 31 | GET | `/admin/families/{id}` | Get family for edit | 🔒 |
| 32 | PUT | `/admin/families/{id}` | Update family | 🔒 |
| 33 | DELETE | `/admin/families/{id}` | Delete family | 🔒 |
| 34 | POST | `/admin/families/validate-exists` | Validate family name | 🔒 |

---

## Tags Endpoints (7)

| # | Method | Endpoint | Purpose | Auth |
|----|--------|----------|---------|------|
| 35 | GET | `/admin/tags` | List tags | 🔒 |
| 36 | GET | `/admin/tags/select` | Get tags for select | 🔒 |
| 37 | POST | `/admin/tags` | Create tag | 🔒 |
| 38 | GET | `/admin/tags/{id}` | Get tag for edit | 🔒 |
| 39 | PUT | `/admin/tags/{id}` | Update tag | 🔒 |
| 40 | DELETE | `/admin/tags/{id}` | Delete tag | 🔒 |
| 41 | POST | `/admin/tags/validate-exists` | Validate tag name | 🔒 |

---

## Seasons Endpoints (7)

| # | Method | Endpoint | Purpose | Auth |
|----|--------|----------|---------|------|
| 42 | GET | `/admin/seasons` | List seasons | 🔒 |
| 43 | GET | `/admin/seasons/select` | Get seasons for select | 🔒 |
| 44 | POST | `/admin/seasons` | Create season | 🔒 |
| 45 | GET | `/admin/seasons/{id}` | Get season for edit | 🔒 |
| 46 | PUT | `/admin/seasons/{id}` | Update season | 🔒 |
| 47 | DELETE | `/admin/seasons/{id}` | Delete season | 🔒 |
| 48 | POST | `/admin/seasons/validate-exists` | Validate season name | 🔒 |

---

## Occasions Endpoints (7)

| # | Method | Endpoint | Purpose | Auth |
|----|--------|----------|---------|------|
| 49 | GET | `/admin/occasions` | List occasions | 🔒 |
| 50 | GET | `/admin/occasions/select` | Get occasions for select | 🔒 |
| 51 | POST | `/admin/occasions` | Create occasion | 🔒 |
| 52 | GET | `/admin/occasions/{id}` | Get occasion for edit | 🔒 |
| 53 | PUT | `/admin/occasions/{id}` | Update occasion | 🔒 |
| 54 | DELETE | `/admin/occasions/{id}` | Delete occasion | 🔒 |
| 55 | POST | `/admin/occasions/validate-exists` | Validate occasion name | 🔒 |

---

## Accords Endpoints (7)

| # | Method | Endpoint | Purpose | Auth |
|----|--------|----------|---------|------|
| 56 | GET | `/admin/accords` | List accords | 🔒 |
| 57 | GET | `/admin/accords/select` | Get accords for select | 🔒 |
| 58 | POST | `/admin/accords` | Create accord | 🔒 |
| 59 | GET | `/admin/accords/{id}` | Get accord for edit | 🔒 |
| 60 | PUT | `/admin/accords/{id}` | Update accord | 🔒 |
| 61 | DELETE | `/admin/accords/{id}` | Delete accord | 🔒 |

---

## Notes Endpoints (8)

| # | Method | Endpoint | Purpose | Auth |
|----|--------|----------|---------|------|
| 62 | GET | `/admin/notes` | List notes | 🔒 |
| 63 | GET | `/admin/notes/categories` | Get note categories | 🔒 |
| 64 | GET | `/admin/notes/select` | Get notes for select | 🔒 |
| 65 | POST | `/admin/notes` | Create note | 🔒 |
| 66 | GET | `/admin/notes/{id}` | Get note for edit | 🔒 |
| 67 | PUT | `/admin/notes/{id}` | Update note | 🔒 |
| 68 | DELETE | `/admin/notes/{id}` | Delete note | 🔒 |
| 69 | POST | `/admin/notes/validate-exists` | Validate note name | 🔒 |

---

## Common Query Parameters

### Pagination (List Endpoints)
```
pageIndex=1          # Default: 1 (1-based, not 0-based)
pageSize=10          # Default: 10, Max: varies by endpoint
```

### Search & Filtering
```
searchTerm=value     # Search by name or keyword
category=value       # For Notes: filter by category
brandId=uuid         # For Perfumes: filter by brand
familyId=uuid        # For Perfumes: filter by family
gender=value         # For Perfumes: filter by gender profile
```

**Example with all parameters:**
```
GET /admin/brands?pageIndex=2&pageSize=20&searchTerm=Dior
```

---

## Common Request Bodies

### Create/Update (Single Resource)
```json
{
  "name": "Resource Name",
  "description": "Optional description",
  "// other fields specific to resource"
}
```

### Create/Update (Perfume - Complex)
```json
{
  "id": "GUID (for updates)",
  "name": "Perfume Name",
  "brandId": "GUID",
  "familyId": "GUID",
  "price": 150.00,
  "description": "Description",
  "stockQuantity": 100,
  "genderProfile": "Unisex",
  "noteIds": ["GUID1", "GUID2"],
  "accordIds": ["GUID1"],
  "seasonIds": ["GUID1"],
  "occasionIds": ["GUID1"],
  "tagIds": ["GUID1"]
}
```

### Validate Existence
```json
{
  "name": "Resource Name",
  "excludeId": "GUID (optional, for updates)"
}
```

### Switch LLM Provider
```json
{
  "providerId": "openai"
}
```

### Login/Register
```json
{
  "email": "user@example.com",
  "password": "Secureadminadmin!",
  "firstName": "John",     // register only
  "lastName": "Doe"        // register only
}
```

---

## HTTP Headers

### Required Headers

**All requests:**
```
Content-Type: application/json
```

**Authorized requests:**
```
Authorization: Bearer <your_jwt_token>
```

### Response Headers
```
Content-Type: application/json
Date: Wed, 19 Mar 2026 10:30:00 GMT
X-Request-Id: uuid
```

---

## HTTP Status Codes

| Code | Meaning | When Used |
|------|---------|-----------|
| 200 | OK | Successful read/update |
| 201 | Created | Successful resource creation |
| 400 | Bad Request | Invalid input, validation errors |
| 401 | Unauthorized | Missing/invalid authentication |
| 403 | Forbidden | Access denied, email not verified |
| 404 | Not Found | Resource doesn't exist |
| 500 | Internal Error | Server error |

---

## Authentication

### Get Token
```bash
POST /accounts/login
Content-Type: application/json

{
  "email": "admin@admin.com",
  "password": "adminadmin"
}

# Response includes token field
```

### Use Token
```bash
GET /admin/dashboard
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Token Format
- JWT Bearer token
- Expires after 24 hours
- Included in `Authorization` header

---

## Response Format Quick Reference

### Success (200)
```json
{
  "success": true,
  "data": { /* response data */ },
  "timestamp": "2026-03-19T10:30:00Z"
}
```

### Validation Error (400)
```json
{
  "success": false,
  "error": "Validation failed",
  "validationErrors": {
    "fieldName": ["Error message"]
  }
}
```

### Not Found (404)
```json
{
  "success": false,
  "error": "Resource not found"
}
```

### Unauthorized (401)
```json
{
  "success": false,
  "error": "Authentication required"
}
```

---

## CRUD Operations Pattern

All catalog management resources follow this pattern:

```
GET    /admin/{resource}              # List with pagination
GET    /admin/{resource}/select       # Get for dropdowns
POST   /admin/{resource}              # Create new
GET    /admin/{resource}/{id}         # Get for editing
PUT    /admin/{resource}/{id}         # Update existing
DELETE /admin/{resource}/{id}         # Delete
POST   /admin/{resource}/validate-exists  # Check name exists
```

Replace `{resource}` with: `brands`, `families`, `tags`, `seasons`, `occasions`, `accords`, `notes`

---

## Common Errors & Solutions

### 401 Unauthorized
**Cause**: Missing or invalid token
**Solution**: Include valid JWT token in Authorization header

### 400 Bad Request - Validation
**Cause**: Invalid input data
**Solution**: Check validationErrors field for specific field errors

### 404 Not Found
**Cause**: Resource doesn't exist
**Solution**: Verify ID is correct and resource exists

### 403 Forbidden (For Login)
**Cause**: Email not verified
**Solution**: Call verify endpoint with code sent to email

### 500 Internal Server Error
**Cause**: Server-side error
**Solution**: Check application logs, contact development team

---

## Typical Admin Workflow

### 1. Login
```bash
POST /accounts/login
→ Get JWT token
```

### 2. Fetch Data
```bash
GET /admin/brands/select
GET /admin/families/select
GET /admin/notes/select
→ Populate dropdown menus
```

### 3. Create/Edit
```bash
POST /admin/perfumes (or PUT for updates)
→ Send perfume with related IDs
```

### 4. Validate Before Save
```bash
POST /admin/brands/validate-exists
→ Check name uniqueness
```

### 5. Dashboard
```bash
GET /admin/dashboard
→ Display statistics
```

---

## Typical Customer Workflow

### 1. Browse
```bash
GET /perfumes?pageIndex=1&pageSize=12
GET /perfumes?brandId=xxx
→ Get product list
```

### 2. View Details
```bash
GET /perfumes/{id}
→ Get full product info
```

### 3. Purchase
```bash
POST /perfumes/add-to-cart
→ Add items to cart
```

---

## Postman Collection

Use the provided `API_POSTMAN_COLLECTION.json` to:
1. Import all 62 endpoints
2. Set up authentication variables
3. Test endpoints with pre-configured examples
4. Generate code snippets

**Import Steps:**
1. Open Postman
2. Click Import
3. Upload `API_POSTMAN_COLLECTION.json`
4. Set `base_url` variable to your API URL
5. Set `token` variable with your JWT token

---

## Base URL References

**Development:**
```
http://localhost:5021/api/v1
```

**Staging:**
```
https://staging.aloud.com/api/v1
```

**Production:**
```
https://api.aloud.com/api/v1
```

---

## Legend

- 🔒 = Requires authentication (Bearer token)
- 🌐 = Public endpoint (no authentication)
- GUID = UUID format identifier
- {id} = Path parameter (replace with actual value)
- [optional] = Optional query/body parameter

---

**Reference Version**: 1.0  
**Last Updated**: 2026-03-19  
**Total Endpoints**: 62  
