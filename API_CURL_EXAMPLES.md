# ALOud REST API - cURL Examples

Complete collection of cURL command examples for testing all 62 REST API endpoints.

**Base URL**: `http://localhost:5021/api/v1`

---

## Setup

### 1. Set Environment Variables

```bash
# Set base URL
BASE_URL="http://localhost:5021/api/v1"

# After login, set token
TOKEN="your_jwt_token_here"

# Some example IDs (replace with actual values)
BRAND_ID="550e8400-e29b-41d4-a716-446655440000"
FAMILY_ID="660e8400-e29b-41d4-a716-446655440001"
PERFUME_ID="770e8400-e29b-41d4-a716-446655440002"
```

### 2. Store Token After Login

```bash
# Save token from login response
export TOKEN=$(curl -s -X POST "$BASE_URL/accounts/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "password123"
  }' | jq -r '.data.token')

echo "Token saved: $TOKEN"
```

---

## Health & Status

### Check Service Health
```bash
curl -X GET "$BASE_URL/health" \
  -H "Content-Type: application/json"
```

---

## Account Operations

### Register New User
```bash
curl -X POST "$BASE_URL/accounts/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "password": "SecurePassword123!",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

### Login User
```bash
curl -X POST "$BASE_URL/accounts/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "password123"
  }' | jq .
```

### Verify Email
```bash
curl -X POST "$BASE_URL/accounts/verify" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "code": "123456"
  }'
```

### Resend Verification Code
```bash
curl -X POST "$BASE_URL/accounts/resend-verification" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com"
  }'
```

### Get User Profile
```bash
curl -X GET "$BASE_URL/accounts/profile" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"
```

---

## Admin Dashboard

### Get Dashboard Statistics
```bash
curl -X GET "$BASE_URL/admin/dashboard" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"
```

### Get LLM Configuration
```bash
curl -X GET "$BASE_URL/admin/llm-config" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"
```

### Switch LLM Provider
```bash
curl -X POST "$BASE_URL/admin/llm-config/switch-provider" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "providerId": "anthropic"
  }'
```

### Get Expert System Configuration
```bash
curl -X GET "$BASE_URL/admin/expert-system" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"
```

---

## Brands Management

### List Brands
```bash
curl -X GET "$BASE_URL/admin/brands?pageIndex=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"
```

### List Brands with Search
```bash
curl -X GET "$BASE_URL/admin/brands?pageIndex=1&pageSize=10&searchTerm=Dior" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"
```

### Get Brands for Select Dropdown
```bash
curl -X GET "$BASE_URL/admin/brands/select" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"
```

### Create Brand
```bash
curl -X POST "$BASE_URL/admin/brands" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Chanel",
    "description": "French luxury brand"
  }'
```

### Get Brand for Edit
```bash
curl -X GET "$BASE_URL/admin/brands/$BRAND_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"
```

### Update Brand
```bash
curl -X PUT "$BASE_URL/admin/brands/$BRAND_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "'$BRAND_ID'",
    "name": "Chanel Couture",
    "description": "Updated description"
  }'
```

### Delete Brand
```bash
curl -X DELETE "$BASE_URL/admin/brands/$BRAND_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"
```

### Validate Brand Name Exists
```bash
curl -X POST "$BASE_URL/admin/brands/validate-exists" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Dior",
    "excludeId": null
  }'
```

### Validate Brand Name (Exclude Current)
```bash
curl -X POST "$BASE_URL/admin/brands/validate-exists" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Dior",
    "excludeId": "'$BRAND_ID'"
  }'
```

---

## Families Management

### List Families
```bash
curl -X GET "$BASE_URL/admin/families?pageIndex=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```

### Get Families for Select
```bash
curl -X GET "$BASE_URL/admin/families/select" \
  -H "Authorization: Bearer $TOKEN"
```

### Create Family
```bash
curl -X POST "$BASE_URL/admin/families" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Floral",
    "description": "Floral fragrance family"
  }'
```

### Get Family for Edit
```bash
curl -X GET "$BASE_URL/admin/families/$FAMILY_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Update Family
```bash
curl -X PUT "$BASE_URL/admin/families/$FAMILY_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "'$FAMILY_ID'",
    "name": "Floral Bouquet",
    "description": "Updated description"
  }'
```

### Delete Family
```bash
curl -X DELETE "$BASE_URL/admin/families/$FAMILY_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Validate Family Name
```bash
curl -X POST "$BASE_URL/admin/families/validate-exists" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Floral"
  }'
```

---

## Tags Management

### List Tags
```bash
curl -X GET "$BASE_URL/admin/tags?pageIndex=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```

### Get Tags for Select
```bash
curl -X GET "$BASE_URL/admin/tags/select" \
  -H "Authorization: Bearer $TOKEN"
```

### Create Tag
```bash
curl -X POST "$BASE_URL/admin/tags" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Luxury",
    "description": "Luxury fragrance tag"
  }'
```

### Get Tag for Edit
```bash
curl -X GET "$BASE_URL/admin/tags/$TAG_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Update Tag
```bash
curl -X PUT "$BASE_URL/admin/tags/$TAG_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "'$TAG_ID'",
    "name": "Premium Luxury",
    "description": "Updated"
  }'
```

### Delete Tag
```bash
curl -X DELETE "$BASE_URL/admin/tags/$TAG_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Validate Tag Name
```bash
curl -X POST "$BASE_URL/admin/tags/validate-exists" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Luxury"
  }'
```

---

## Seasons Management

### List Seasons
```bash
curl -X GET "$BASE_URL/admin/seasons?pageIndex=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```

### Get Seasons for Select
```bash
curl -X GET "$BASE_URL/admin/seasons/select" \
  -H "Authorization: Bearer $TOKEN"
```

### Create Season
```bash
curl -X POST "$BASE_URL/admin/seasons" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Winter",
    "description": "Winter season fragrances"
  }'
```

### Get Season for Edit
```bash
curl -X GET "$BASE_URL/admin/seasons/$SEASON_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Update Season
```bash
curl -X PUT "$BASE_URL/admin/seasons/$SEASON_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "'$SEASON_ID'",
    "name": "Winter Elegance",
    "description": "Updated"
  }'
```

### Delete Season
```bash
curl -X DELETE "$BASE_URL/admin/seasons/$SEASON_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Validate Season Name
```bash
curl -X POST "$BASE_URL/admin/seasons/validate-exists" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Winter"
  }'
```

---

## Occasions Management

### List Occasions
```bash
curl -X GET "$BASE_URL/admin/occasions?pageIndex=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```

### Get Occasions for Select
```bash
curl -X GET "$BASE_URL/admin/occasions/select" \
  -H "Authorization: Bearer $TOKEN"
```

### Create Occasion
```bash
curl -X POST "$BASE_URL/admin/occasions" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Evening",
    "description": "Evening wear fragrances"
  }'
```

### Get Occasion for Edit
```bash
curl -X GET "$BASE_URL/admin/occasions/$OCCASION_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Update Occasion
```bash
curl -X PUT "$BASE_URL/admin/occasions/$OCCASION_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "'$OCCASION_ID'",
    "name": "Evening Gala",
    "description": "Updated"
  }'
```

### Delete Occasion
```bash
curl -X DELETE "$BASE_URL/admin/occasions/$OCCASION_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Validate Occasion Name
```bash
curl -X POST "$BASE_URL/admin/occasions/validate-exists" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Evening"
  }'
```

---

## Accords Management

### List Accords
```bash
curl -X GET "$BASE_URL/admin/accords?pageIndex=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```

### Get Accords for Select
```bash
curl -X GET "$BASE_URL/admin/accords/select" \
  -H "Authorization: Bearer $TOKEN"
```

### Create Accord
```bash
curl -X POST "$BASE_URL/admin/accords" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Woody",
    "description": "Woody accord notes"
  }'
```

### Get Accord for Edit
```bash
curl -X GET "$BASE_URL/admin/accords/$ACCORD_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Update Accord
```bash
curl -X PUT "$BASE_URL/admin/accords/$ACCORD_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "'$ACCORD_ID'",
    "name": "Deep Woody",
    "description": "Updated"
  }'
```

### Delete Accord
```bash
curl -X DELETE "$BASE_URL/admin/accords/$ACCORD_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Validate Accord Name
```bash
curl -X POST "$BASE_URL/admin/accords/validate-exists" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Woody"
  }'
```

---

## Notes Management

### List Notes
```bash
curl -X GET "$BASE_URL/admin/notes?pageIndex=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```

### List Notes with Category Filter
```bash
curl -X GET "$BASE_URL/admin/notes?pageIndex=1&pageSize=10&category=Wood" \
  -H "Authorization: Bearer $TOKEN"
```

### Get Note Categories
```bash
curl -X GET "$BASE_URL/admin/notes/categories" \
  -H "Authorization: Bearer $TOKEN"
```

### Get Notes for Select
```bash
curl -X GET "$BASE_URL/admin/notes/select" \
  -H "Authorization: Bearer $TOKEN"
```

### Create Note
```bash
curl -X POST "$BASE_URL/admin/notes" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Oud",
    "category": "Wood",
    "description": "Precious oud wood note"
  }'
```

### Get Note for Edit
```bash
curl -X GET "$BASE_URL/admin/notes/$NOTE_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Update Note
```bash
curl -X PUT "$BASE_URL/admin/notes/$NOTE_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "'$NOTE_ID'",
    "name": "Premium Oud",
    "category": "Wood",
    "description": "Updated"
  }'
```

### Delete Note
```bash
curl -X DELETE "$BASE_URL/admin/notes/$NOTE_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Validate Note Name
```bash
curl -X POST "$BASE_URL/admin/notes/validate-exists" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Oud"
  }'
```

---

## Perfumes Management (Admin)

### List Perfumes (Admin)
```bash
curl -X GET "$BASE_URL/perfumes/admin?pageIndex=1&pageSize=10" \
  -H "Authorization: Bearer $TOKEN"
```

### List Perfumes with Filters
```bash
curl -X GET "$BASE_URL/perfumes/admin?pageIndex=1&pageSize=10&query=Oud&gender=Unisex" \
  -H "Authorization: Bearer $TOKEN"
```

### Get Perfumes for Select
```bash
curl -X GET "$BASE_URL/perfumes/admin/select" \
  -H "Authorization: Bearer $TOKEN"
```

### Create Perfume
```bash
curl -X POST "$BASE_URL/perfumes/admin" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Black Oud Premium",
    "brandId": "'$BRAND_ID'",
    "familyId": "'$FAMILY_ID'",
    "price": 175.00,
    "description": "Premium oud fragrance",
    "stockQuantity": 100,
    "genderProfile": "Unisex",
    "noteIds": [],
    "accordIds": [],
    "seasonIds": [],
    "occasionIds": [],
    "tagIds": []
  }'
```

### Get Perfume for Edit
```bash
curl -X GET "$BASE_URL/perfumes/admin/$PERFUME_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Update Perfume
```bash
curl -X PUT "$BASE_URL/perfumes/admin/$PERFUME_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "'$PERFUME_ID'",
    "name": "Black Oud Deluxe",
    "brandId": "'$BRAND_ID'",
    "familyId": "'$FAMILY_ID'",
    "price": 180.00,
    "description": "Updated premium oud",
    "stockQuantity": 75,
    "genderProfile": "Unisex",
    "noteIds": [],
    "accordIds": [],
    "seasonIds": [],
    "occasionIds": [],
    "tagIds": []
  }'
```

### Delete Perfume
```bash
curl -X DELETE "$BASE_URL/perfumes/admin/$PERFUME_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Validate Perfume Name
```bash
curl -X POST "$BASE_URL/perfumes/admin/validate-exists" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Black Oud"
  }'
```

---

## Perfumes (Customer)

### Get Perfumes (Public)
```bash
curl -X GET "$BASE_URL/perfumes?pageIndex=1&pageSize=12"
```

### Get Perfumes with Filters
```bash
curl -X GET "$BASE_URL/perfumes?pageIndex=1&pageSize=12&query=Oud" \
  -H "Content-Type: application/json"
```

### Get Perfume Details
```bash
curl -X GET "$BASE_URL/perfumes/$PERFUME_ID" \
  -H "Content-Type: application/json"
```

### Add to Cart
```bash
curl -X POST "$BASE_URL/perfumes/add-to-cart" \
  -H "Content-Type: application/json" \
  -d '{
    "perfumeId": "'$PERFUME_ID'",
    "quantity": 2
  }'
```

---

## Useful Tips

### Pretty Print JSON Response
Add `| jq` to any command:
```bash
curl -X GET "$BASE_URL/admin/brands" \
  -H "Authorization: Bearer $TOKEN" | jq .
```

### Extract Specific Fields
```bash
# Extract just the brand names
curl -X GET "$BASE_URL/admin/brands/select" \
  -H "Authorization: Bearer $TOKEN" | jq '.data[].name'

# Extract token from login
curl -X POST "$BASE_URL/accounts/login" \
  -H "Content-Type: application/json" \
  -d '{...}' | jq '.data.token'
```

### Save Response to File
```bash
curl -X GET "$BASE_URL/admin/brands" \
  -H "Authorization: Bearer $TOKEN" > brands.json
```

### Check Response Headers
Add `-i` flag:
```bash
curl -i -X GET "$BASE_URL/admin/brands" \
  -H "Authorization: Bearer $TOKEN"
```

### Count Returned Items
```bash
curl -s -X GET "$BASE_URL/admin/brands" \
  -H "Authorization: Bearer $TOKEN" | jq '.data | length'
```

### Create Multiple Resources (Loop)
```bash
# Create 5 brands
for i in {1..5}; do
  curl -X POST "$BASE_URL/admin/brands" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '{
      "name": "Brand'$i'",
      "description": "Test brand '$i'"
    }'
  echo "Brand $i created"
done
```

---

## Batch Operations

### List All Resources Across Pages
```bash
#!/bin/bash
BASE_URL="http://localhost:5021/api/v1"
TOKEN="your_token"
PAGE=1
PAGE_SIZE=100

while true; do
  RESPONSE=$(curl -s -X GET "$BASE_URL/admin/brands?pageIndex=$PAGE&pageSize=$PAGE_SIZE" \
    -H "Authorization: Bearer $TOKEN")
  
  # Check if we have more pages
  TOTAL_PAGES=$(echo $RESPONSE | jq '.data.totalPages')
  
  if [ $PAGE -ge $TOTAL_PAGES ]; then
    break
  fi
  
  PAGE=$((PAGE + 1))
done
```

### Delete All Items
```bash
#!/bin/bash
BASE_URL="http://localhost:5021/api/v1"
TOKEN="your_token"

# Get all brand IDs
IDS=$(curl -s -X GET "$BASE_URL/admin/brands?pageSize=1000" \
  -H "Authorization: Bearer $TOKEN" | jq -r '.data.items[].id')

# Delete each one
for ID in $IDS; do
  curl -X DELETE "$BASE_URL/admin/brands/$ID" \
    -H "Authorization: Bearer $TOKEN"
  echo "Deleted $ID"
done
```

---

## Troubleshooting

### Test Endpoint Availability
```bash
# Test if API is running
curl -X GET "$BASE_URL/health"

# Check with verbose output
curl -v -X GET "$BASE_URL/health"
```

### Debug Authentication
```bash
# Test with token
curl -v -X GET "$BASE_URL/admin/dashboard" \
  -H "Authorization: Bearer $TOKEN" | jq '.data.error'
```

### Validate JSON Request
```bash
# Use jq to validate JSON before sending
echo '{
  "name": "Test",
  "description": "Test"
}' | jq '.'
```

---

**Last Updated**: 2026-03-19  
**Version**: 1.0  
**Total Examples**: 100+
