# ALOud REST API - Documentation Index

Complete API documentation covering all 62 REST endpoints for the ALOud perfume e-commerce platform.

**Last Updated**: 2026-03-19  
**API Version**: v1  
**Status**: ✅ Production Ready

---

## 📚 Documentation Files

### 1. **API_FULL_DOCUMENTATION.md** (Main Reference)
Complete API documentation with:
- ✅ Authentication & Authorization
- ✅ Response format specifications  
- ✅ All 62 endpoints with full details
- ✅ Request/response examples for each endpoint
- ✅ Query parameters & path parameters
- ✅ Error handling & status codes
- ✅ Pagination guide
- ✅ Rate limiting recommendations

**Use this for**: 
- Understanding endpoint specifications
- Learning request/response formats
- Integration testing
- Reference during development

---

### 2. **API_QUICK_REFERENCE.md** (Developer Lookup)
Quick reference guide with:
- ✅ All 62 endpoints in tabular format
- ✅ Common query parameters
- ✅ Common request bodies
- ✅ HTTP headers reference
- ✅ Status codes table
- ✅ CRUD operations pattern
- ✅ Typical workflows (admin vs customer)

**Use this for**:
- Quick endpoint lookup
- Common parameter reference
- Understanding API patterns
- During development as cheat sheet

---

### 3. **API_CURL_EXAMPLES.md** (Testing Guide)
Complete cURL command examples:
- ✅ 100+ tested command examples
- ✅ Environment setup instructions
- ✅ Authentication token management
- ✅ All endpoints with working examples
- ✅ Batch operation scripts
- ✅ Troubleshooting commands
- ✅ Useful tips & tricks

**Use this for**:
- Testing endpoints manually
- Learning API interaction patterns
- Running batch operations
- Debugging issues
- Copy-paste ready commands

---

### 4. **API_POSTMAN_COLLECTION.json** (Automated Testing)
Postman collection file containing:
- ✅ All 62 endpoints pre-configured
- ✅ Pre-set authentication headers
- ✅ Example request bodies
- ✅ Environment variables setup
- ✅ Pre-request scripts
- ✅ Test assertions

**Use this for**:
- Importing into Postman
- Automated endpoint testing
- Team collaboration
- CI/CD integration testing
- Generating code snippets

**Import Steps**:
1. Open Postman
2. Click "Import"
3. Select `API_POSTMAN_COLLECTION.json`
4. Set environment variables (base_url, token)
5. Start testing

---

## 🎯 Quick Navigation

### By Use Case

**I want to...**

#### Test an endpoint quickly
→ See **API_CURL_EXAMPLES.md**

#### Understand the full API
→ See **API_FULL_DOCUMENTATION.md**

#### Find an endpoint name
→ See **API_QUICK_REFERENCE.md**

#### Import into Postman
→ See **API_POSTMAN_COLLECTION.json**

#### Learn authentication
→ See **API_FULL_DOCUMENTATION.md** → Authentication section

#### Integrate with frontend
→ See **API_FULL_DOCUMENTATION.md** → All sections + Typical Workflows

---

### By Endpoint Type

#### Admin Endpoints (4)
- Dashboard
- LLM Configuration
- Expert System
See: **API_FULL_DOCUMENTATION.md** → Admin Endpoints

#### Account Endpoints (5)
- Register
- Login
- Email verification
- User profile
See: **API_FULL_DOCUMENTATION.md** → Account Endpoints

#### Perfume Management (10)
- Admin CRUD
- Customer view/search
- Add to cart
See: **API_FULL_DOCUMENTATION.md** → Perfumes Endpoints

#### Catalog Management (40)
- Brands (7)
- Families (7)
- Tags (7)
- Seasons (7)
- Occasions (7)
- Accords (7)
- Notes (8)
See: **API_FULL_DOCUMENTATION.md** → Catalog Sections

---

## 📊 API Statistics

### Endpoint Count
| Type | Count |
|------|-------|
| Admin | 4 |
| Account | 5 |
| Perfumes | 10 |
| Brands | 7 |
| Families | 7 |
| Tags | 7 |
| Seasons | 7 |
| Occasions | 7 |
| Accords | 7 |
| Notes | 8 |
| Health | 1 |
| **TOTAL** | **62** |

### Authentication
- 🔒 Authorized (JWT Bearer): **46 endpoints**
- 🌐 Public: **16 endpoints**

### HTTP Methods
| Method | Count |
|--------|-------|
| GET | 26 |
| POST | 19 |
| PUT | 13 |
| DELETE | 4 |

---

## 🚀 Getting Started

### 1. Setup Your Environment

```bash
# Set base URL
export BASE_URL="http://localhost:5021/api/v1"

# Check health
curl -X GET "$BASE_URL/health"
```

### 2. Register & Login

```bash
# Register
curl -X POST "$BASE_URL/accounts/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "SecurePassword123!",
    "firstName": "Admin",
    "lastName": "User"
  }'

# Login
TOKEN=$(curl -s -X POST "$BASE_URL/accounts/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "SecurePassword123!"
  }' | jq -r '.data.token')

export TOKEN
```

### 3. Test an Endpoint

```bash
# Get dashboard (requires auth)
curl -X GET "$BASE_URL/admin/dashboard" \
  -H "Authorization: Bearer $TOKEN"

# Get public perfumes list (no auth)
curl -X GET "$BASE_URL/perfumes?pageIndex=1&pageSize=12"
```

### 4. Use Postman

1. Import `API_POSTMAN_COLLECTION.json`
2. Set `base_url` variable
3. Use Login endpoint to get token
4. Set `token` variable
5. Test all endpoints

---

## 🔐 Authentication

### JWT Bearer Token

All admin endpoints require authentication.

**Header Format:**
```
Authorization: Bearer <your_jwt_token>
```

**How to get token:**

1. Register: `POST /accounts/register`
2. Or Login: `POST /accounts/login`
3. Extract `token` from response
4. Use in `Authorization` header

**Token Expiration**: 24 hours

See **API_FULL_DOCUMENTATION.md** → Authentication for details

---

## 📝 Response Format

### Success Response
```json
{
  "success": true,
  "data": { /* response data */ },
  "timestamp": "2026-03-19T10:30:00Z"
}
```

### Error Response
```json
{
  "success": false,
  "error": "Error description",
  "code": "ERROR_CODE"
}
```

### Validation Error
```json
{
  "success": false,
  "error": "Validation failed",
  "validationErrors": {
    "fieldName": ["Error message"]
  }
}
```

---

## 🔄 Common Patterns

### CRUD Pattern (Brands, Families, Tags, Seasons, Occasions, Accords, Notes)

```
GET    /admin/{resource}              List with pagination
GET    /admin/{resource}/select       Get for dropdowns
POST   /admin/{resource}              Create new
GET    /admin/{resource}/{id}         Get for editing
PUT    /admin/{resource}/{id}         Update existing
DELETE /admin/{resource}/{id}         Delete
POST   /admin/{resource}/validate-exists  Check name uniqueness
```

### Query Parameters (List Endpoints)

```
?pageIndex=1         # Page number (1-based)
&pageSize=10         # Items per page
&searchTerm=value    # Search filter
&category=value      # Category filter (Notes only)
```

### Path Parameters

```
/{id}                # Replace with actual GUID
```

---

## ✅ Testing Checklist

### Before Deployment

- [ ] All 62 endpoints responding correctly
- [ ] Authentication working (401 for unauthenticated admin endpoints)
- [ ] Pagination working correctly
- [ ] Search/filter parameters working
- [ ] CRUD operations working (Create, Read, Update, Delete)
- [ ] Validation errors returning proper format
- [ ] 404 errors for non-existent resources
- [ ] 400 errors for invalid input
- [ ] 500 errors with proper error messages
- [ ] Unique name validation working
- [ ] Response times acceptable (< 500ms)
- [ ] CORS headers configured correctly

### Manual Testing with cURL

```bash
# 1. Health check
curl -X GET "$BASE_URL/health"

# 2. Register and login
# See API_CURL_EXAMPLES.md for full flow

# 3. Test one CRUD resource
# See API_CURL_EXAMPLES.md for Brands example

# 4. Test customer endpoints
curl -X GET "$BASE_URL/perfumes?pageIndex=1&pageSize=12"
```

### Automated Testing with Postman

1. Import collection
2. Set environment variables
3. Run collection tests
4. Export test results

---

## 🐛 Troubleshooting

### Common Issues

#### 401 Unauthorized
- Missing `Authorization` header
- Invalid or expired token
- Token not included for protected endpoint
**Solution**: Get valid token from login endpoint

#### 400 Bad Request
- Invalid JSON in request body
- Missing required field
- Invalid data type or format
**Solution**: Check `validationErrors` field in response

#### 404 Not Found
- Resource ID doesn't exist
- Wrong endpoint path
- Typo in URL
**Solution**: Verify ID exists before requesting

#### 500 Internal Server Error
- Server error
- Database connection issue
- Unexpected exception
**Solution**: Check application logs

#### Connection Refused
- API not running
- Wrong host/port
- Firewall blocking connection
**Solution**: Verify API is running at correct URL

---

## 📚 Related Documentation

### Existing Docs in Repository

- `ENDPOINT_TEST_REPORT.md` - Test report with expected responses
- `API_POSTMAN_COLLECTION.json` - Postman collection
- `EXECUTIVE_SUMMARY.md` - Project overview
- `MIGRATION_ANALYSIS.md` - Technical analysis

---

## 🔗 Base URLs

| Environment | URL |
|-------------|-----|
| Development | `http://localhost:5021/api/v1` |
| Staging | `https://staging.aloud.com/api/v1` |
| Production | `https://api.aloud.com/api/v1` |

---

## 📞 Support

### Getting Help

1. **Check Documentation**
   - Read full documentation in `API_FULL_DOCUMENTATION.md`
   - Check quick reference in `API_QUICK_REFERENCE.md`

2. **Test with cURL**
   - Use examples from `API_CURL_EXAMPLES.md`
   - Verify response format

3. **Use Postman**
   - Import `API_POSTMAN_COLLECTION.json`
   - Use pre-configured requests

4. **Check Application Logs**
   - Server logs for errors
   - Request/response traces

5. **Contact Development Team**
   - Report issues with specific endpoints
   - Include request/response examples

---

## 📋 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-03-19 | Initial release - 62 endpoints |

---

## ✨ Features

### ✅ Implemented
- [x] 62 REST endpoints
- [x] JWT authentication
- [x] Pagination support
- [x] Search/filter support
- [x] Input validation
- [x] Error handling
- [x] Admin & public endpoints
- [x] Catalog management (7 resource types)
- [x] Perfume management
- [x] Account management

### 🔄 Future Enhancements
- [ ] Rate limiting
- [ ] API versioning (v2)
- [ ] WebSocket support for real-time updates
- [ ] GraphQL support
- [ ] OpenAPI/Swagger UI
- [ ] API key authentication

---

## 📄 License

This documentation and API are part of the ALOud e-commerce platform.

---

## 🎓 Learning Path

### Beginner
1. Read: API_QUICK_REFERENCE.md
2. Try: Health endpoint with cURL
3. Try: Public perfume endpoints

### Intermediate
1. Read: API_FULL_DOCUMENTATION.md (Authentication section)
2. Register & login with cURL
3. Try: Admin catalog endpoints
4. Import Postman collection

### Advanced
1. Test all 62 endpoints
2. Implement in frontend application
3. Set up continuous integration testing
4. Monitor performance metrics

---

**Documentation Version**: 1.0  
**Status**: Complete & Production Ready  
**Last Updated**: 2026-03-19  

For detailed information, see the specific documentation files listed above.
