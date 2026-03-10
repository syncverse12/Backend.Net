# ⚠️ SyncVerse API - Error Codes & Response Format

## 📋 Standard Response Format

All API endpoints return responses in the following format:

### Success Response
```json
{
  "isSuccess": true,
  "message": "Operation completed successfully",
  "data": {
    // Response data here
  }
}
```

### Error Response
```json
{
  "isSuccess": false,
  "message": "Error message describing what went wrong",
  "errors": [
    "Detailed error 1",
    "Detailed error 2"
  ]
}
```

---

## 🔢 HTTP Status Codes

| Code | Status | Description |
|------|--------|-------------|
| 200 | OK | Request succeeded |
| 201 | Created | Resource created successfully |
| 400 | Bad Request | Validation failed or invalid input |
| 401 | Unauthorized | Missing or invalid authentication token |
| 403 | Forbidden | User lacks permission for this action |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Resource already exists or state conflict |
| 500 | Internal Server Error | Server error occurred |

---

## 🚨 Common Error Scenarios

### 1. Authentication Errors

#### 401 - Missing Token
```json
{
  "isSuccess": false,
  "message": "Authorization token is required"
}
```
**Solution:** Include `Authorization: Bearer {token}` header

#### 401 - Invalid Token
```json
{
  "isSuccess": false,
  "message": "Invalid authentication token"
}
```
**Solution:** Login again to get a new token

#### 401 - Expired Token
```json
{
  "isSuccess": false,
  "message": "Token has expired"
}
```
**Solution:** Use refresh token or re-authenticate

---

### 2. Validation Errors (400)

#### Registration Validation
```json
{
  "isSuccess": false,
  "message": "Validation failed",
  "errors": [
    "Email is required",
    "Password must be at least 8 characters",
    "Phone number format is invalid"
  ]
}
```

#### Project Creation Validation
```json
{
  "isSuccess": false,
  "message": "Validation failed",
  "errors": [
    "Project name is required",
    "Start date must be before end date",
    "Workspace ID is invalid"
  ]
}
```

#### File Upload Validation
```json
{
  "isSuccess": false,
  "message": "File validation failed",
  "errors": [
    "File size exceeds 10MB limit",
    "File type not supported. Allowed: .pdf, .doc, .jpg, .png"
  ]
}
```

---

### 3. Authorization Errors (403)

#### Manager-Only Action
```json
{
  "isSuccess": false,
  "message": "You do not have permission to perform this action",
  "errors": ["This action requires Manager role"]
}
```

#### Project Access Denied
```json
{
  "isSuccess": false,
  "message": "Access denied",
  "errors": ["You are not a member of this project"]
}
```

#### Task Not Owned
```json
{
  "isSuccess": false,
  "message": "Access denied",
  "errors": ["You can only modify tasks assigned to you"]
}
```

---

### 4. Not Found Errors (404)

#### Resource Not Found
```json
{
  "isSuccess": false,
  "message": "Project not found",
  "errors": ["No project exists with ID: {projectId}"]
}
```

#### User Not Found
```json
{
  "isSuccess": false,
  "message": "User not found",
  "errors": ["No user exists with email: {email}"]
}
```

---

### 5. Conflict Errors (409)

#### Email Already Exists
```json
{
  "isSuccess": false,
  "message": "Registration failed",
  "errors": ["Email address is already registered"]
}
```

#### Duplicate Resource
```json
{
  "isSuccess": false,
  "message": "Workspace already exists",
  "errors": ["A workspace with this name already exists"]
}
```

#### Invalid State Transition
```json
{
  "isSuccess": false,
  "message": "Cannot change task status",
  "errors": ["Task must be 'In Progress' before it can be submitted"]
}
```

---

### 6. Server Errors (500)

```json
{
  "isSuccess": false,
  "message": "An unexpected error occurred",
  "errors": ["Internal server error. Please try again later."]
}
```

---

## 🎯 Domain-Specific Errors

### Authentication & Authorization

| Error Code | Message | Solution |
|------------|---------|----------|
| `AUTH_001` | Invalid OTP | Enter correct OTP from email |
| `AUTH_002` | OTP expired | Request new OTP |
| `AUTH_003` | Invalid credentials | Check email/password |
| `AUTH_004` | Email not verified | Verify email first |
| `AUTH_005` | Account locked | Contact support |

**Example:**
```json
{
  "isSuccess": false,
  "message": "Invalid OTP",
  "errorCode": "AUTH_001",
  "errors": ["The OTP you entered is incorrect or has expired"]
}
```

---

### Project Management

| Error Code | Message | Solution |
|------------|---------|----------|
| `PROJ_001` | Project not found | Check project ID |
| `PROJ_002` | Access denied | Request project access |
| `PROJ_003` | Project deleted | Cannot modify deleted project |
| `PROJ_004` | Invalid date range | End date must be after start date |
| `PROJ_005` | Workspace required | Specify valid workspace ID |

---

### Task Management

| Error Code | Message | Solution |
|------------|---------|----------|
| `TASK_001` | Task not found | Check task ID |
| `TASK_002` | Not assigned to you | Only assigned user can modify |
| `TASK_003` | Invalid status transition | Follow task lifecycle |
| `TASK_004` | Dependency not met | Complete dependent tasks first |
| `TASK_005` | Cannot delete in-progress task | Change status before deleting |

---

### File Management

| Error Code | Message | Solution |
|------------|---------|----------|
| `FILE_001` | File too large | Max size: 10MB |
| `FILE_002` | Invalid file type | Use allowed formats |
| `FILE_003` | Upload failed | Retry upload |
| `FILE_004` | File not found | Check file ID |
| `FILE_005` | Storage limit exceeded | Free up space or upgrade |

---

### Invitations

| Error Code | Message | Solution |
|------------|---------|----------|
| `INV_001` | Invitation not found | Check invitation ID |
| `INV_002` | Invitation expired | Request new invitation |
| `INV_003` | Already a member | User already in project |
| `INV_004` | Cannot invite manager | Only employees can be invited |

---

## 🛠️ Error Handling in Code

### JavaScript/TypeScript
```typescript
async function handleApiCall() {
  try {
    const response = await apiClient.post('/api/projects', projectData);
    return response.data;
  } catch (error) {
    if (error.response?.data) {
      const { message, errors, errorCode } = error.response.data;
      
      // Handle specific error codes
      switch (errorCode) {
        case 'AUTH_001':
          showError('Invalid OTP. Please check your email.');
          break;
        case 'PROJ_002':
          showError('You do not have access to this project.');
          redirectToProjects();
          break;
        default:
          // Show generic error
          showError(message || 'An error occurred');
          if (errors && errors.length > 0) {
            errors.forEach(err => console.error(err));
          }
      }
    }
  }
}
```

### Flutter/Dart
```dart
Future<void> handleApiCall() async {
  try {
    final response = await apiClient.post('/api/projects', projectData);
    // Success handling
  } on DioException catch (e) {
    if (e.response?.data != null) {
      final errorData = e.response!.data;
      final message = errorData['message'] ?? 'An error occurred';
      final errorCode = errorData['errorCode'];
      final errors = errorData['errors'] as List<dynamic>?;
      
      switch (errorCode) {
        case 'AUTH_001':
          showSnackbar('Invalid OTP. Please check your email.');
          break;
        case 'PROJ_002':
          showSnackbar('You do not have access to this project.');
          Navigator.pushReplacementNamed(context, '/projects');
          break;
        default:
          showSnackbar(message);
          if (errors != null) {
            errors.forEach(print);
          }
      }
    }
  }
}
```

### Unity/C#
```csharp
void HandleApiResponse(UnityWebRequest request)
{
    if (request.result == UnityWebRequest.Result.Success)
    {
        // Success handling
    }
    else
    {
        var errorData = JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text);
        
        switch (errorData.errorCode)
        {
            case "AUTH_001":
                ShowError("Invalid OTP. Please check your email.");
                break;
            case "PROJ_002":
                ShowError("You do not have access to this project.");
                LoadScene("ProjectsScene");
                break;
            default:
                ShowError(errorData.message ?? "An error occurred");
                break;
        }
    }
}

[System.Serializable]
public class ErrorResponse
{
    public bool isSuccess;
    public string message;
    public string errorCode;
    public string[] errors;
}
```

---

## 🧪 Testing Error Scenarios

### Using Postman

1. **Test Unauthorized Access:**
   - Remove `Authorization` header
   - Expected: 401 with auth error

2. **Test Validation Errors:**
   - Send empty/invalid data
   - Expected: 400 with validation errors

3. **Test Forbidden Access:**
   - Login as Employee
   - Call Manager-only endpoint
   - Expected: 403 with permission error

4. **Test Not Found:**
   - Use non-existent ID
   - Expected: 404 with not found error

---

## 📊 Error Logging & Monitoring

### Frontend Error Tracking
```typescript
// Log errors to monitoring service
function logError(error: any) {
  console.error('API Error:', {
    endpoint: error.config?.url,
    status: error.response?.status,
    message: error.response?.data?.message,
    errorCode: error.response?.data?.errorCode,
    errors: error.response?.data?.errors,
    timestamp: new Date().toISOString(),
  });
  
  // Send to monitoring service (e.g., Sentry, LogRocket)
  // Sentry.captureException(error);
}
```

---

## 🆘 Troubleshooting Guide

### "Unauthorized" - Token Issues
1. Check token is present in Authorization header
2. Verify token format: `Bearer {token}`
3. Check token hasn't expired
4. Ensure user has proper role

### "Validation Failed" - Input Issues
1. Check all required fields are provided
2. Verify data types match expected format
3. Check field length constraints
4. Validate email/phone formats

### "Access Denied" - Permission Issues
1. Verify user role (Manager vs Employee)
2. Check project membership
3. Confirm task ownership
4. Review authorization policies

### "Not Found" - Resource Issues
1. Verify resource ID is correct
2. Check if resource was deleted
3. Ensure you have access to the resource
4. Confirm workspace/project context

---

## 📞 Support

For unresolved errors:
1. Check API logs in server console
2. Review Swagger documentation: `http://localhost:5131/swagger`
3. Test endpoint in Postman
4. Contact backend team with error details

---

**Last Updated:** March 2024
