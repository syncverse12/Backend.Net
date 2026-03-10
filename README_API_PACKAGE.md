# 📦 SyncVerse API - Integration Package

## 🎯 Overview

Complete API integration package for **Frontend**, **Flutter**, and **Gaming** teams to integrate with SyncVerse Project Management System.

### 🌟 Key Features
- ✅ 68 REST API Endpoints
- ✅ Real-time Notifications (SignalR)
- ✅ JWT Authentication
- ✅ File Upload/Download
- ✅ Role-based Authorization (Manager/Employee)
- ✅ 3D Virtual Office Support
- ✅ AI Team Recommendations (Coming Soon)

---

## 📄 Package Contents

### 🔹 Quick Start
📘 **`QUICK_START_GUIDE.md`** - Start here! Team-specific setup instructions

### 🔹 API Documentation
- 📘 **`SyncVerse_API_Collection.postman_collection.json`** - 68 API endpoints
- 📘 **`SyncVerse_API_Environment.postman_environment.json`** - Environment variables
- 📘 **`POSTMAN_COLLECTION_README.md`** - Postman setup guide

### 🔹 Integration Guides
- 📗 **`API_INTEGRATION_GUIDE.md`** - Code examples (React, Flutter, Unity)
- 📗 **`SIGNALR_INTEGRATION_GUIDE.md`** - Real-time notifications setup
- 📗 **`ERROR_CODES_DOCUMENTATION.md`** - Error handling reference

---

## 🚀 Quick Links by Team

### 🌐 Frontend Team (React/Vue/Angular)
1. Read **`QUICK_START_GUIDE.md`** → Frontend section
2. Import Postman files
3. Copy code from **`API_INTEGRATION_GUIDE.md`** → React section
4. Setup SignalR from **`SIGNALR_INTEGRATION_GUIDE.md`**

### 📱 Flutter Team
1. Read **`QUICK_START_GUIDE.md`** → Flutter section
2. Add dependencies (Dio, SignalR)
3. Copy code from **`API_INTEGRATION_GUIDE.md`** → Flutter section
4. Test with Postman first!

### 🎮 Gaming Team (Unity)
1. Read **`QUICK_START_GUIDE.md`** → Gaming section
2. Install NuGet packages
3. Copy code from **`API_INTEGRATION_GUIDE.md`** → Unity section
4. Setup SignalR for 3D position tracking

---

## 🔑 Authentication Flow (All Teams)

```
1. Register → 2. Verify Email (OTP) → 3. Login → 4. Get JWT Token → 5. Use in API calls
```

**Example Login Request:**
```json
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "Password@123"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "userId": "...",
    "role": "Manager",
    "fullName": "Ahmed Mohamed"
  }
}
```

**Use Token:**
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

---

## 📊 API Endpoints Overview

### 🔐 Authentication (6 endpoints)
- Register, Login, Verify Email, Forgot Password

### 🏢 Workspaces (6 endpoints)
- CRUD operations for workspaces (Manager only)

### 📁 Projects (11 endpoints)
- Manager: Create, update, delete, invite employees
- Employee: View projects, accept/reject invitations

### 🎯 Milestones (6 endpoints)
- Create and track project milestones

### ✅ Tasks (15 endpoints)
- Manager: Assign, review, approve/reject tasks
- Employee: Start, submit tasks

### 💬 Comments (4 endpoints)
- Add, update, delete task comments

### ⏱️ Time Tracking (7 endpoints)
- Start/stop time logs, manual entries

### 📎 File Attachments (8 endpoints)
- Upload/download files for projects and tasks

### 🔔 Notifications (5 endpoints)
- Get, mark as read, delete notifications

### 📂 Categories (3 endpoints)
- Task categorization

**Total: 68 REST API Endpoints**

---

## 🔔 Real-time Features (SignalR)

### WebSocket Endpoint
```
ws://localhost:5131/hubs/notifications?access_token={jwt_token}
```

### Events
- `ReceiveNotification` - New task, comment, invitation, etc.

### Supported Platforms
- ✅ JavaScript/TypeScript (Web)
- ✅ Flutter/Dart (Mobile)
- ✅ Unity/C# (Gaming)

See **`SIGNALR_INTEGRATION_GUIDE.md`** for complete setup.

---

## 🌐 API Server Info

### Development
```
Base URL (HTTP):  http://localhost:5131
Base URL (HTTPS): https://localhost:7001
Swagger UI:       http://localhost:5131/swagger
SignalR Hub:      ws://localhost:5131/hubs/notifications
```

### CORS (Development)
Allowed origins:
- `http://localhost:3000` (React/Next.js)
- `http://localhost:4200` (Angular)
- `http://localhost:8080` (Vue.js)
- `http://localhost:5173` (Vite)
- `capacitor://localhost` (Mobile)

### Test Account
```
Email: admin@syncverse.com
Password: Admin@123
Role: Manager
```

---

## 💻 Code Examples

### JavaScript (React)
```typescript
import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'http://localhost:5131',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

// Get projects
const projects = await apiClient.get('/api/employee/projects');
```

### Flutter (Dart)
```dart
import 'package:dio/dio.dart';

final dio = Dio(BaseOptions(
  baseUrl: 'http://localhost:5131',
  headers: {'Authorization': 'Bearer $token'},
));

// Get projects
final response = await dio.get('/api/employee/projects');
final projects = response.data['data'];
```

### Unity (C#)
```csharp
using UnityEngine.Networking;

StartCoroutine(GetProjects());

IEnumerator GetProjects() {
    using (UnityWebRequest request = UnityWebRequest.Get(
        "http://localhost:5131/api/employee/projects"
    )) {
        request.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success) {
            Debug.Log(request.downloadHandler.text);
        }
    }
}
```

See **`API_INTEGRATION_GUIDE.md`** for complete examples!

---

## ⚠️ Error Handling

### Standard Error Response
```json
{
  "isSuccess": false,
  "message": "Error message",
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

### HTTP Status Codes
- `200` - Success
- `400` - Validation error
- `401` - Unauthorized (invalid token)
- `403` - Forbidden (insufficient permissions)
- `404` - Not found
- `500` - Server error

See **`ERROR_CODES_DOCUMENTATION.md`** for detailed error handling.

---

## ✅ Integration Checklist

### Step 1: Setup Postman
- [ ] Import collection JSON
- [ ] Import environment JSON
- [ ] Test Register → Login flow
- [ ] Save JWT token

### Step 2: Setup HTTP Client
- [ ] Install dependencies (axios/dio/etc.)
- [ ] Create API client with base URL
- [ ] Add Authorization header interceptor
- [ ] Implement error handling

### Step 3: Implement Authentication
- [ ] Register screen
- [ ] Login screen
- [ ] Token storage (localStorage/SecureStorage)
- [ ] Auto-logout on token expiry

### Step 4: Implement Core Features
- [ ] Display user projects
- [ ] Display user tasks
- [ ] Upload files
- [ ] Handle notifications

### Step 5: Add Real-time (Optional)
- [ ] Setup SignalR connection
- [ ] Listen for notifications
- [ ] Show toast/alerts
- [ ] Handle reconnection

---

## 🧪 Testing Guide

### 1. Test with Postman First
Before writing any code, test all endpoints in Postman to understand:
- Request/response format
- Required headers
- Expected errors

### 2. Test Authentication Flow
```
Register → Verify Email → Login → Get JWT → Use JWT in requests
```

### 3. Test Main Features
- Create workspace
- Create project
- Invite employee
- Create task
- Upload file
- Get notifications

### 4. Test Error Scenarios
- Missing token (401)
- Invalid data (400)
- Unauthorized action (403)
- Not found (404)

---

## 🎯 Next Steps

### For Frontend Team
1. Setup React/Vue/Angular project
2. Install axios or fetch wrapper
3. Copy ApiClient from integration guide
4. Build auth screens
5. Integrate real-time notifications

### For Flutter Team
1. Setup Flutter project
2. Add Dio and SignalR dependencies
3. Copy API client code
4. Create models for entities
5. Build UI screens

### For Gaming Team
1. Setup Unity project
2. Install NuGet packages
3. Copy UnityWebRequest wrapper
4. Create 3D virtual office
5. Integrate position tracking

---

## 📞 Support & Resources

### Documentation
- **Swagger UI:** http://localhost:5131/swagger (when API is running)
- **Postman Collection:** Complete API reference with examples
- **Integration Guides:** Framework-specific code samples

### Contact
- **Backend Team:** For API issues, bugs, or questions
- **GitHub Repo:** https://github.com/syncverse12/Backend.Net

### Helpful Tips
1. Always test with Postman before coding
2. Read error messages carefully
3. Check Swagger for endpoint details
4. Use provided code examples as starting point
5. Don't hesitate to ask questions!

---

## 🚀 Ready to Build?

**Start Here:**
1. Open **`QUICK_START_GUIDE.md`**
2. Follow your team's section (Frontend/Flutter/Gaming)
3. Import Postman files
4. Copy code examples
5. Build awesome features! 🎉

---

## 📋 File Summary

| File | Purpose | For |
|------|---------|-----|
| `README.md` | This file - Package overview | Everyone |
| `QUICK_START_GUIDE.md` | Team-specific quick start | Everyone |
| `API_INTEGRATION_GUIDE.md` | Code examples & patterns | Developers |
| `SIGNALR_INTEGRATION_GUIDE.md` | Real-time setup | Developers |
| `ERROR_CODES_DOCUMENTATION.md` | Error handling | Developers |
| `POSTMAN_COLLECTION_README.md` | Postman setup | Everyone |
| `*.postman_*.json` | Postman files | Everyone |

---

## 🎨 Project Features

### ✅ Current Features
- REST API with 68 endpoints
- JWT authentication with OTP
- Role-based authorization
- File upload/download
- Real-time notifications (SignalR)
- Time tracking
- Task dependencies
- Soft delete & restore

### 🚀 Upcoming Features
- 🎮 3D Virtual Office Environment
- 🤖 AI Team Recommendations
- 📊 Advanced Analytics
- 🎯 Kanban Board
- 📈 Gantt Charts

---

**Version:** 1.0.0  
**Last Updated:** March 2024  
**Made with ❤️ by SyncVerse Team**
