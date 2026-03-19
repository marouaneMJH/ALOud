# ALOud REST API Documentation

Complete REST API documentation for the ALOud perfume e-commerce platform. All 62 endpoints across 9 controllers with detailed specifications, request/response examples, and authentication requirements.

**Base URL**: `http://localhost:5021/api/v1` (or your deployment URL)

**Version**: 1.0

**Last Updated**: 2026-03-19

---

## Table of Contents

1. [Authentication](#authentication)
2. [Response Format](#response-format)
3. [Health Endpoints](#health-endpoints)
4. [Admin Endpoints](#admin-endpoints)
5. [Account Endpoints](#account-endpoints)
6. [Perfumes Endpoints](#perfumes-endpoints)
7. [Brands Endpoints](#brands-endpoints)
8. [Families Endpoints](#families-endpoints)
9. [Tags Endpoints](#tags-endpoints)
10. [Seasons Endpoints](#seasons-endpoints)
11. [Occasions Endpoints](#occasions-endpoints)
12. [Accords Endpoints](#accords-endpoints)
13. [Notes Endpoints](#notes-endpoints)
14. [Error Handling](#error-handling)
15. [Pagination](#pagination)

---

## Authentication

### Bearer Token Authentication

All admin endpoints (marked with 🔒 **[AUTHORIZE]**) require JWT Bearer token authentication.

**Header Format:**
```
Authorization: Bearer <your_jwt_token>
```

**How to Obtain Token:**
1. Register or login using Account endpoints
2. Receive JWT token in response
3. Include token in Authorization header for subsequent requests

**Token Expiration:**
- Tokens expire after 24 hours
- Implement token refresh logic on the client side
- Return 401 Unauthorized when token expires

---

## Response Format

### Success Response (200-299)

All successful responses follow this format:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    // Response data here
  },
  "timestamp": "2026-03-19T10:30:00Z"
}
```

### Error Response (400-599)

```json
{
  "success": false,
  "error": "Error message describing the issue",
  "code": "ERROR_CODE",
  "timestamp": "2026-03-19T10:30:00Z"
}
```

### Validation Error Response (400)

```json
{
  "success": false,
  "error": "Validation failed",
  "validationErrors": {
    "fieldName": ["Error message 1", "Error message 2"],
    "anotherField": ["Error message"]
  },
  "timestamp": "2026-03-19T10:30:00Z"
}
```

---

## Health Endpoints

### Check Service Health

**Endpoint:** `GET /health`

**Authentication:** None

**Description:** Check if the API service is running and healthy.

**Response:**
```json
{
  "status": "ok",
  "timestamp": "2026-03-19T10:30:00Z",
  "service": "aloud-store"
}
```

**Response Codes:**
- `200 OK` - Service is healthy
- `500 Internal Server Error` - Service health check failed

**Example Request:**
```bash
curl -X GET "http://localhost:5021/api/v1/health"
```

---

## Admin Endpoints

### 1. Get Dashboard Statistics

**Endpoint:** `GET /admin/dashboard`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Retrieves dashboard statistics and KPIs for admin panel.

**Query Parameters:** None

**Response:**
```json
{
  "success": true,
  "data": {
    "totalProducts": 156,
    "totalUsers": 1243,
    "totalOrders": 5678,
    "totalRevenue": 125430.50,
    "averageOrderValue": 22.05,
    "recentOrders": [],
    "topProducts": [],
    "userGrowth": []
  }
}
```

**Response Codes:**
- `200 OK` - Dashboard statistics retrieved
- `401 Unauthorized` - Authentication required
- `500 Internal Server Error` - Server error

**Example Request:**
```bash
curl -X GET "http://localhost:5021/api/v1/admin/dashboard" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

### 2. Get LLM Configuration

**Endpoint:** `GET /admin/llm-config`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Retrieves available LLM providers and current configuration.

**Query Parameters:** None

**Response:**
```json
{
  "success": true,
  "data": {
    "providers": [
      {
        "id": "openai",
        "name": "OpenAI GPT-4",
        "isActive": true,
        "description": "OpenAI's GPT-4 model"
      },
      {
        "id": "anthropic",
        "name": "Anthropic Claude",
        "isActive": false,
        "description": "Anthropic's Claude model"
      }
    ]
  }
}
```

**Response Codes:**
- `200 OK` - LLM providers retrieved
- `401 Unauthorized` - Authentication required
- `500 Internal Server Error` - Server error

**Example Request:**
```bash
curl -X GET "http://localhost:5021/api/v1/admin/llm-config" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

### 3. Switch LLM Provider

**Endpoint:** `POST /admin/llm-config/switch-provider`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Switches the active LLM provider to a different one.

**Request Body:**
```json
{
  "providerId": "anthropic"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "message": "Successfully switched to anthropic",
    "note": "The change will take effect on the next request"
  }
}
```

**Response Codes:**
- `200 OK` - Provider switched successfully
- `400 Bad Request` - Invalid provider ID or missing data
- `401 Unauthorized` - Authentication required
- `500 Internal Server Error` - Server error

**Validation Rules:**
- `providerId` is required and cannot be empty
- `providerId` must be a valid provider ID

**Example Request:**
```bash
curl -X POST "http://localhost:5021/api/v1/admin/llm-config/switch-provider" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"providerId": "anthropic"}'
```

---

### 4. Get Expert System Configuration

**Endpoint:** `GET /admin/expert-system`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Retrieves expert system configuration and rules.

**Query Parameters:** None

**Response:**
```json
{
  "success": true,
  "data": {
    "message": "Expert system configuration",
    "config": {}
  }
}
```

**Response Codes:**
- `200 OK` - Expert system configuration retrieved
- `401 Unauthorized` - Authentication required
- `500 Internal Server Error` - Server error

**Example Request:**
```bash
curl -X GET "http://localhost:5021/api/v1/admin/expert-system" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## Account Endpoints

### 1. Register New User

**Endpoint:** `POST /accounts/register`

**Authentication:** None

**Description:** Register a new user account.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "message": "Registration successful. Please check your email for verification code.",
    "email": "user@example.com"
  }
}
```

**Response Codes:**
- `200 OK` - Registration successful
- `400 Bad Request` - Invalid data or validation errors
- `500 Internal Server Error` - Server error

**Validation Rules:**
- `email` is required and must be valid email format
- `email` must be unique (not already registered)
- `password` is required and minimum 8 characters
- `firstName` and `lastName` are required

**Example Request:**
```bash
curl -X POST "http://localhost:5021/api/v1/accounts/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

---

### 2. User Login

**Endpoint:** `POST /accounts/login`

**Authentication:** None

**Description:** Authenticate user and receive JWT token.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "message": "Login successful",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe"
    },
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

**Response Codes:**
- `200 OK` - Login successful
- `400 Bad Request` - Invalid data or validation errors
- `401 Unauthorized` - Invalid credentials
- `403 Forbidden` - Email not verified

**Validation Rules:**
- `email` is required and must be valid
- `password` is required

**Example Request:**
```bash
curl -X POST "http://localhost:5021/api/v1/accounts/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!"
  }'
```

---

### 3. Verify Email

**Endpoint:** `POST /accounts/verify`

**Authentication:** None

**Description:** Verify user's email with the verification code sent to their inbox.

**Request Body:**
```json
{
  "email": "user@example.com",
  "code": "123456"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "message": "Email verified successfully"
  }
}
```

**Response Codes:**
- `200 OK` - Email verified successfully
- `400 Bad Request` - Invalid or expired code
- `404 Not Found` - User not found

**Validation Rules:**
- `email` is required
- `code` is required

**Example Request:**
```bash
curl -X POST "http://localhost:5021/api/v1/accounts/verify" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "code": "123456"
  }'
```

---

### 4. Resend Verification Code

**Endpoint:** `POST /accounts/resend-verification`

**Authentication:** None

**Description:** Resend verification code to user's email.

**Request Body:**
```json
{
  "email": "user@example.com"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "message": "Verification code sent successfully"
  }
}
```

**Response Codes:**
- `200 OK` - Verification code sent
- `400 Bad Request` - Invalid email
- `404 Not Found` - User not found

**Validation Rules:**
- `email` is required and must be valid

**Example Request:**
```bash
curl -X POST "http://localhost:5021/api/v1/accounts/resend-verification" \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com"}'
```

---

### 5. Get User Profile

**Endpoint:** `GET /accounts/profile`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Get current user's profile information.

**Query Parameters:** None

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "isEmailVerified": true
  }
}
```

**Response Codes:**
- `200 OK` - Profile retrieved successfully
- `401 Unauthorized` - Authentication required
- `404 Not Found` - User not found

**Example Request:**
```bash
curl -X GET "http://localhost:5021/api/v1/accounts/profile" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## Perfumes Endpoints

### Admin Perfume Management

#### 1. List Perfumes (Admin)

**Endpoint:** `GET /perfumes/admin`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** List all perfumes with pagination, search, and filtering. Admin only.

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| query | string | null | Search term for perfume name or brand |
| brandId | GUID | null | Filter by brand ID |
| familyId | GUID | null | Filter by family ID |
| gender | string | null | Filter by gender profile |
| pageIndex | int | 1 | Page number (1-based) |
| pageSize | int | 12 | Items per page |

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "name": "Black Oud",
        "brand": "Dior",
        "price": 150.00,
        "stockQuantity": 50,
        "description": "Elegant and mysterious"
      }
    ],
    "totalCount": 156,
    "pageIndex": 1,
    "pageSize": 12,
    "totalPages": 13
  }
}
```

**Response Codes:**
- `200 OK` - Perfumes retrieved
- `400 Bad Request` - Invalid query parameters
- `401 Unauthorized` - Authentication required

**Example Request:**
```bash
curl -X GET "http://localhost:5021/api/v1/perfumes/admin?pageIndex=1&pageSize=10&query=Oud" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

#### 2. Get Perfume Select List (Admin)

**Endpoint:** `GET /perfumes/admin/select`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Get simplified list of all perfumes for dropdowns.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Black Oud"
    },
    {
      "id": "660e8400-e29b-41d4-a716-446655440001",
      "name": "Sauvage"
    }
  ]
}
```

**Response Codes:**
- `200 OK` - Select list retrieved
- `401 Unauthorized` - Authentication required

**Example Request:**
```bash
curl -X GET "http://localhost:5021/api/v1/perfumes/admin/select" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

#### 3. Create Perfume

**Endpoint:** `POST /perfumes/admin`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Create a new perfume.

**Request Body:**
```json
{
  "name": "Black Oud Premium",
  "brandId": "550e8400-e29b-41d4-a716-446655440000",
  "familyId": "660e8400-e29b-41d4-a716-446655440001",
  "price": 175.00,
  "description": "Premium oud fragrance",
  "stockQuantity": 100,
  "genderProfile": "Unisex",
  "noteIds": ["550e8400-e29b-41d4-a716-446655440002"],
  "accordIds": ["660e8400-e29b-41d4-a716-446655440003"],
  "seasonIds": [],
  "occasionIds": [],
  "tagIds": []
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "770e8400-e29b-41d4-a716-446655440004",
    "message": "Perfume created successfully"
  }
}
```

**Response Codes:**
- `201 Created` - Perfume created successfully
- `400 Bad Request` - Validation errors
- `401 Unauthorized` - Authentication required

**Example Request:**
```bash
curl -X POST "http://localhost:5021/api/v1/perfumes/admin" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{...request body...}'
```

---

#### 4. Get Perfume for Edit

**Endpoint:** `GET /perfumes/admin/{id}`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Get perfume details for editing.

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| id | GUID | Perfume ID |

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Black Oud",
    "brandId": "660e8400-e29b-41d4-a716-446655440001",
    "price": 150.00,
    "description": "Elegant and mysterious",
    "stockQuantity": 50,
    "genderProfile": "Unisex",
    "notes": [],
    "accords": [],
    "tags": []
  }
}
```

**Response Codes:**
- `200 OK` - Perfume retrieved
- `401 Unauthorized` - Authentication required
- `404 Not Found` - Perfume not found

**Example Request:**
```bash
curl -X GET "http://localhost:5021/api/v1/perfumes/admin/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

#### 5. Update Perfume

**Endpoint:** `PUT /perfumes/admin/{id}`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Update an existing perfume.

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| id | GUID | Perfume ID |

**Request Body:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Black Oud Deluxe",
  "brandId": "550e8400-e29b-41d4-a716-446655440000",
  "familyId": "660e8400-e29b-41d4-a716-446655440001",
  "price": 180.00,
  "description": "Updated premium oud fragrance",
  "stockQuantity": 75,
  "genderProfile": "Unisex",
  "noteIds": [],
  "accordIds": [],
  "seasonIds": [],
  "occasionIds": [],
  "tagIds": []
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "message": "Perfume updated successfully"
  }
}
```

**Response Codes:**
- `200 OK` - Perfume updated
- `400 Bad Request` - Validation errors or ID mismatch
- `401 Unauthorized` - Authentication required
- `404 Not Found` - Perfume not found

**Example Request:**
```bash
curl -X PUT "http://localhost:5021/api/v1/perfumes/admin/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{...request body...}'
```

---

#### 6. Delete Perfume

**Endpoint:** `DELETE /perfumes/admin/{id}`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Delete a perfume.

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| id | GUID | Perfume ID |

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "message": "Perfume deleted successfully"
  }
}
```

**Response Codes:**
- `200 OK` - Perfume deleted
- `401 Unauthorized` - Authentication required
- `404 Not Found` - Perfume not found

**Example Request:**
```bash
curl -X DELETE "http://localhost:5021/api/v1/perfumes/admin/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

#### 7. Validate Perfume Existence

**Endpoint:** `POST /perfumes/admin/validate-exists`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Check if a perfume name already exists.

**Request Body:**
```json
{
  "name": "Black Oud",
  "excludeId": null
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "exists": true
  }
}
```

**Response Codes:**
- `200 OK` - Validation completed
- `400 Bad Request` - Name required
- `401 Unauthorized` - Authentication required

**Example Request:**
```bash
curl -X POST "http://localhost:5021/api/v1/perfumes/admin/validate-exists" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "Black Oud"}'
```

---

### Customer Perfume Endpoints

#### 8. Get Perfumes (Customer)

**Endpoint:** `GET /perfumes`

**Authentication:** None

**Description:** Get paginated list of perfumes with filters. Public endpoint.

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| query | string | null | Search term |
| brandId | GUID | null | Brand filter |
| familyId | GUID | null | Family filter |
| gender | string | null | Gender filter |
| pageIndex | int | 1 | Page number |
| pageSize | int | 12 | Items per page |

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "name": "Black Oud",
        "price": 150.00,
        "description": "Elegant fragrance"
      }
    ],
    "totalCount": 156,
    "pageIndex": 1,
    "pageSize": 12,
    "totalPages": 13
  }
}
```

**Example Request:**
```bash
curl -X GET "http://localhost:5021/api/v1/perfumes?pageIndex=1&pageSize=12"
```

---

#### 9. Get Perfume Details

**Endpoint:** `GET /perfumes/{id}`

**Authentication:** None

**Description:** Get detailed information for a specific perfume.

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| id | GUID | Perfume ID |

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Black Oud",
    "brand": "Dior",
    "price": 150.00,
    "description": "Elegant and mysterious oud fragrance",
    "stockQuantity": 50,
    "notes": ["Oud", "Sandalwood"],
    "accords": ["Woody", "Oriental"],
    "imageUrl": "https://example.com/image.jpg"
  }
}
```

**Response Codes:**
- `200 OK` - Perfume retrieved
- `404 Not Found` - Perfume not found

**Example Request:**
```bash
curl -X GET "http://localhost:5021/api/v1/perfumes/550e8400-e29b-41d4-a716-446655440000"
```

---

#### 10. Add to Cart

**Endpoint:** `POST /perfumes/add-to-cart`

**Authentication:** None

**Description:** Add perfume to shopping cart.

**Request Body:**
```json
{
  "perfumeId": "550e8400-e29b-41d4-a716-446655440000",
  "quantity": 2
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "message": "Black Oud added to cart"
  }
}
```

**Response Codes:**
- `200 OK` - Item added to cart
- `400 Bad Request` - Invalid quantity or insufficient stock
- `404 Not Found` - Perfume not found

**Validation Rules:**
- `perfumeId` is required
- `quantity` must be greater than 0
- Stock must be sufficient

**Example Request:**
```bash
curl -X POST "http://localhost:5021/api/v1/perfumes/add-to-cart" \
  -H "Content-Type: application/json" \
  -d '{"perfumeId": "550e8400-e29b-41d4-a716-446655440000", "quantity": 2}'
```

---

## Brands Endpoints

### 1. List Brands

**Endpoint:** `GET /admin/brands`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** List all brands with pagination and search.

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| pageIndex | int | 1 | Page number |
| pageSize | int | 10 | Items per page |
| searchTerm | string | null | Search term |

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "name": "Dior",
        "description": "Luxury fashion brand"
      }
    ],
    "totalCount": 24,
    "pageIndex": 1,
    "pageSize": 10,
    "totalPages": 3
  }
}
```

**Example Request:**
```bash
curl -X GET "http://localhost:5021/api/v1/admin/brands?pageIndex=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

### 2. Get Brands for Select

**Endpoint:** `GET /admin/brands/select`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Get simplified list of brands for dropdowns.

**Response:**
```json
{
  "success": true,
  "data": [
    {"id": "550e8400-e29b-41d4-a716-446655440000", "name": "Dior"},
    {"id": "660e8400-e29b-41d4-a716-446655440001", "name": "Guerlain"}
  ]
}
```

**Example Request:**
```bash
curl -X GET "http://localhost:5021/api/v1/admin/brands/select" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

### 3. Create Brand

**Endpoint:** `POST /admin/brands`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Create a new brand.

**Request Body:**
```json
{
  "name": "Chanel",
  "description": "French luxury brand"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Brand created successfully"
}
```

**Response Codes:** `201 Created`, `400 Bad Request`, `401 Unauthorized`

**Example Request:**
```bash
curl -X POST "http://localhost:5021/api/v1/admin/brands" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "Chanel", "description": "French luxury brand"}'
```

---

### 4. Get Brand for Edit

**Endpoint:** `GET /admin/brands/{id}`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Get brand details for editing.

**Path Parameters:** `id` (GUID)

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Dior",
    "description": "Luxury fashion brand"
  }
}
```

**Example Request:**
```bash
curl -X GET "http://localhost:5021/api/v1/admin/brands/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

### 5. Update Brand

**Endpoint:** `PUT /admin/brands/{id}`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Update an existing brand.

**Path Parameters:** `id` (GUID)

**Request Body:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Dior Couture",
  "description": "Updated luxury brand"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Brand updated successfully"
}
```

**Example Request:**
```bash
curl -X PUT "http://localhost:5021/api/v1/admin/brands/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{...request body...}'
```

---

### 6. Delete Brand

**Endpoint:** `DELETE /admin/brands/{id}`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Delete a brand.

**Path Parameters:** `id` (GUID)

**Response:**
```json
{
  "success": true,
  "message": "Brand deleted successfully"
}
```

**Example Request:**
```bash
curl -X DELETE "http://localhost:5021/api/v1/admin/brands/550e8400-e29b-41d4-a716-446655440000" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

### 7. Validate Brand Existence

**Endpoint:** `POST /admin/brands/validate-exists`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Check if a brand name already exists.

**Request Body:**
```json
{
  "name": "Dior",
  "excludeId": null
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "exists": true
  }
}
```

**Example Request:**
```bash
curl -X POST "http://localhost:5021/api/v1/admin/brands/validate-exists" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "Dior"}'
```

---

## Families Endpoints

All endpoints follow the same pattern as Brands. Base path: `/admin/families`

### Family Endpoints Summary

| Method | Endpoint | Purpose | Auth |
|--------|----------|---------|------|
| GET | `/admin/families` | List families | 🔒 |
| GET | `/admin/families/select` | Get for dropdown | 🔒 |
| POST | `/admin/families` | Create family | 🔒 |
| GET | `/admin/families/{id}` | Get for edit | 🔒 |
| PUT | `/admin/families/{id}` | Update family | 🔒 |
| DELETE | `/admin/families/{id}` | Delete family | 🔒 |
| POST | `/admin/families/validate-exists` | Check name exists | 🔒 |

**Example Request (List):**
```bash
curl -X GET "http://localhost:5021/api/v1/admin/families" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## Tags Endpoints

All endpoints follow the same pattern as Brands. Base path: `/admin/tags`

### Tags Endpoints Summary

| Method | Endpoint | Purpose | Auth |
|--------|----------|---------|------|
| GET | `/admin/tags` | List tags | 🔒 |
| GET | `/admin/tags/select` | Get for dropdown | 🔒 |
| POST | `/admin/tags` | Create tag | 🔒 |
| GET | `/admin/tags/{id}` | Get for edit | 🔒 |
| PUT | `/admin/tags/{id}` | Update tag | 🔒 |
| DELETE | `/admin/tags/{id}` | Delete tag | 🔒 |
| POST | `/admin/tags/validate-exists` | Check name exists | 🔒 |

---

## Seasons Endpoints

All endpoints follow the same pattern as Brands. Base path: `/admin/seasons`

### Seasons Endpoints Summary

| Method | Endpoint | Purpose | Auth |
|--------|----------|---------|------|
| GET | `/admin/seasons` | List seasons | 🔒 |
| GET | `/admin/seasons/select` | Get for dropdown | 🔒 |
| POST | `/admin/seasons` | Create season | 🔒 |
| GET | `/admin/seasons/{id}` | Get for edit | 🔒 |
| PUT | `/admin/seasons/{id}` | Update season | 🔒 |
| DELETE | `/admin/seasons/{id}` | Delete season | 🔒 |
| POST | `/admin/seasons/validate-exists` | Check name exists | 🔒 |

---

## Occasions Endpoints

All endpoints follow the same pattern as Brands. Base path: `/admin/occasions`

### Occasions Endpoints Summary

| Method | Endpoint | Purpose | Auth |
|--------|----------|---------|------|
| GET | `/admin/occasions` | List occasions | 🔒 |
| GET | `/admin/occasions/select` | Get for dropdown | 🔒 |
| POST | `/admin/occasions` | Create occasion | 🔒 |
| GET | `/admin/occasions/{id}` | Get for edit | 🔒 |
| PUT | `/admin/occasions/{id}` | Update occasion | 🔒 |
| DELETE | `/admin/occasions/{id}` | Delete occasion | 🔒 |
| POST | `/admin/occasions/validate-exists` | Check name exists | 🔒 |

---

## Accords Endpoints

All endpoints follow the same pattern as Brands. Base path: `/admin/accords`

### Accords Endpoints Summary

| Method | Endpoint | Purpose | Auth |
|--------|----------|---------|------|
| GET | `/admin/accords` | List accords | 🔒 |
| GET | `/admin/accords/select` | Get for dropdown | 🔒 |
| POST | `/admin/accords` | Create accord | 🔒 |
| GET | `/admin/accords/{id}` | Get for edit | 🔒 |
| PUT | `/admin/accords/{id}` | Update accord | 🔒 |
| DELETE | `/admin/accords/{id}` | Delete accord | 🔒 |
| POST | `/admin/accords/validate-exists` | Check name exists | 🔒 |

---

## Notes Endpoints

Base path: `/admin/notes`

### 1. List Notes

**Endpoint:** `GET /admin/notes`

**Authentication:** 🔒 **[AUTHORIZE]**

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| pageIndex | int | 1 | Page number |
| pageSize | int | 10 | Items per page |
| searchTerm | string | null | Search term |
| category | string | null | Note category filter |

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {"id": "...", "name": "Oud", "category": "Wood"}
    ],
    "totalCount": 150,
    "pageIndex": 1,
    "pageSize": 10,
    "totalPages": 15
  }
}
```

**Example Request:**
```bash
curl -X GET "http://localhost:5021/api/v1/admin/notes?category=Wood" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

### 2. Get Note Categories

**Endpoint:** `GET /admin/notes/categories`

**Authentication:** 🔒 **[AUTHORIZE]**

**Description:** Get available note categories.

**Response:**
```json
{
  "success": true,
  "data": {
    "categories": ["Wood", "Floral", "Citrus", "Oriental"]
  }
}
```

**Example Request:**
```bash
curl -X GET "http://localhost:5021/api/v1/admin/notes/categories" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

### 3. Get Notes for Select

**Endpoint:** `GET /admin/notes/select`

**Authentication:** 🔒 **[AUTHORIZE]**

**Response:**
```json
{
  "success": true,
  "data": [
    {"id": "...", "name": "Oud"},
    {"id": "...", "name": "Sandalwood"}
  ]
}
```

---

### 4. Create Note

**Endpoint:** `POST /admin/notes`

**Authentication:** 🔒 **[AUTHORIZE]**

**Request Body:**
```json
{
  "name": "Oud",
  "category": "Wood",
  "description": "Precious wood note"
}
```

---

### 5. Get Note for Edit

**Endpoint:** `GET /admin/notes/{id}`

**Authentication:** 🔒 **[AUTHORIZE]**

---

### 6. Update Note

**Endpoint:** `PUT /admin/notes/{id}`

**Authentication:** 🔒 **[AUTHORIZE]**

**Request Body:**
```json
{
  "id": "...",
  "name": "Oud Premium",
  "category": "Wood",
  "description": "Premium oud note"
}
```

---

### 7. Delete Note

**Endpoint:** `DELETE /admin/notes/{id}`

**Authentication:** 🔒 **[AUTHORIZE]**

---

### 8. Validate Note Existence

**Endpoint:** `POST /admin/notes/validate-exists`

**Authentication:** 🔒 **[AUTHORIZE]**

**Request Body:**
```json
{
  "name": "Oud",
  "excludeId": null
}
```

---

## Error Handling

### Common HTTP Status Codes

| Code | Meaning | Response |
|------|---------|----------|
| 200 | OK | Success |
| 201 | Created | Resource created |
| 400 | Bad Request | Invalid input or validation failed |
| 401 | Unauthorized | Authentication required |
| 403 | Forbidden | Access denied or email not verified |
| 404 | Not Found | Resource not found |
| 500 | Internal Server Error | Server error |

### Error Response Examples

**Validation Error (400):**
```json
{
  "success": false,
  "error": "Validation failed",
  "validationErrors": {
    "name": ["Name is required", "Name cannot be empty"],
    "email": ["Email must be valid format"]
  }
}
```

**Authentication Error (401):**
```json
{
  "success": false,
  "error": "Invalid credentials",
  "code": "AUTHENTICATION_FAILED"
}
```

**Not Found Error (404):**
```json
{
  "success": false,
  "error": "Brand with ID 550e8400-e29b-41d4-a716-446655440000 not found"
}
```

**Server Error (500):**
```json
{
  "success": false,
  "error": "An unexpected error occurred",
  "code": "INTERNAL_SERVER_ERROR"
}
```

---

## Pagination

### Pagination Format

All list endpoints return paginated responses:

```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 156,
    "pageIndex": 1,
    "pageSize": 10,
    "totalPages": 16
  }
}
```

### Query Parameters

| Parameter | Type | Default | Max | Description |
|-----------|------|---------|-----|-------------|
| pageIndex | int | 1 | N/A | Page number (1-based) |
| pageSize | int | 10 | 100 | Items per page |

### Example: Get Page 3 with 20 items per page

```bash
curl -X GET "http://localhost:5021/api/v1/admin/brands?pageIndex=3&pageSize=20" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Calculating Total Pages

```
totalPages = CEIL(totalCount / pageSize)
```

Example: If `totalCount = 156` and `pageSize = 10`, then `totalPages = 16`

---

## Rate Limiting

Currently, there are no rate limits implemented. Recommended limits for production:

- **Admin endpoints**: 100 requests per minute per user
- **Public endpoints**: 1000 requests per minute per IP
- **Authentication endpoints**: 10 requests per minute per IP (to prevent brute force)

---

## CORS Configuration

The API should be configured to accept requests from your frontend domain.

**Example CORS Headers:**
```
Access-Control-Allow-Origin: https://yourdomain.com
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS
Access-Control-Allow-Headers: Content-Type, Authorization
Access-Control-Allow-Credentials: true
```

---

## API Summary

### Endpoint Count by Type

| Type | Count |
|------|-------|
| Admin Endpoints | 4 |
| Account Endpoints | 5 |
| Perfume Endpoints | 10 |
| Brands Endpoints | 7 |
| Families Endpoints | 7 |
| Tags Endpoints | 7 |
| Seasons Endpoints | 7 |
| Occasions Endpoints | 7 |
| Accords Endpoints | 7 |
| Notes Endpoints | 8 |
| Health Endpoints | 1 |
| **TOTAL** | **62** |

### Authentication Summary

| Type | Required | Count |
|------|----------|-------|
| 🔒 Authorized | Yes | 46 |
| 🌐 Public | No | 16 |

---

## Support & Documentation

For issues or questions:
1. Check this documentation
2. Review error responses for details
3. Check application logs for server errors
4. Contact the development team

**Documentation Version**: 1.0  
**Last Updated**: 2026-03-19  
**API Version**: v1  
