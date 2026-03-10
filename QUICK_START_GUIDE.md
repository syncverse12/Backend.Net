# 🚀 SyncVerse API - Quick Start for All Teams

## 📦 Package Contents

You've received the following files:

### 📄 API Documentation
1. ✅ **`SyncVerse_API_Collection.postman_collection.json`** - Complete API endpoints (68 endpoints)
2. ✅ **`SyncVerse_API_Environment.postman_environment.json`** - Environment variables
3. ✅ **`POSTMAN_COLLECTION_README.md`** - Postman setup guide

### 📚 Integration Guides
4. ✅ **`API_INTEGRATION_GUIDE.md`** - Framework-specific examples (React, Flutter, Unity)
5. ✅ **`SIGNALR_INTEGRATION_GUIDE.md`** - Real-time notifications setup
6. ✅ **`ERROR_CODES_DOCUMENTATION.md`** - Error handling reference
7. ✅ **`QUICK_START_GUIDE.md`** - This file

---

## 🎯 Team-Specific Quick Starts

### 🌐 Frontend Team (React/Angular/Vue)

#### Prerequisites
- Node.js 18+
- npm or yarn
- Your favorite framework

#### Step 1: Test API with Postman
```bash
1. Import SyncVerse_API_Collection.postman_collection.json
2. Import SyncVerse_API_Environment.postman_environment.json
3. Test Register → Login → Get Projects flow
```

#### Step 2: Setup HTTP Client
```bash
npm install axios
# or
npm install @tanstack/react-query  # For advanced data fetching
```

#### Step 3: Copy Integration Code
See **`API_INTEGRATION_GUIDE.md`** → React/Next.js section for:
- API Client setup
- Auth service
- Projects service
- Error handling

#### Step 4: Setup SignalR (Optional - for real-time)
```bash
npm install @microsoft/signalr
```
See **`SIGNALR_INTEGRATION_GUIDE.md`** for connection code.

#### Step 5: Start Building! 🎉
```typescript
// Example: Get user's projects
const projects = await apiClient.get('/api/employee/projects');
console.log(projects);
```

---

### 📱 Flutter Team (Mobile)

#### Prerequisites
- Flutter 3.0+
- Dart 3.0+

#### Step 1: Add Dependencies
```yaml
# pubspec.yaml
dependencies:
  dio: ^5.4.0
  flutter_secure_storage: ^9.0.0
  flutter_riverpod: ^2.4.9  # Or your state management
  signalr_netcore: ^1.3.6   # For real-time
```

#### Step 2: Test API with Postman
```bash
1. Import collection and environment files
2. Test all endpoints you'll use
3. Note the response structures
```

#### Step 3: Copy Integration Code
See **`API_INTEGRATION_GUIDE.md`** → Flutter section for:
- Dio HTTP client setup
- Auth repository
- Projects provider
- SignalR connection

#### Step 4: Create Models
```dart
// Example: Project model
class Project {
  final String id;
  final String name;
  final String description;
  
  Project.fromJson(Map<String, dynamic> json)
    : id = json['id'],
      name = json['name'],
      description = json['description'];
}
```

#### Step 5: Build UI! 🎉
```dart
// Example: Display projects
FutureBuilder<List<Project>>(
  future: projectsRepository.getAll(),
  builder: (context, snapshot) {
    if (snapshot.hasData) {
      return ProjectsList(projects: snapshot.data!);
    }
    return CircularProgressIndicator();
  },
)
```

---

### 🎮 Gaming Team (Unity)

#### Prerequisites
- Unity 2021.3+
- .NET Standard 2.1

#### Step 1: Install NuGet Packages
```
1. Open Unity Package Manager
2. Install NuGet for Unity
3. Add these packages:
   - Newtonsoft.Json (for JSON parsing)
   - Microsoft.AspNetCore.SignalR.Client (for real-time)
```

#### Step 2: Test API with Postman
```bash
1. Import collection and environment files
2. Test authentication flow
3. Test 3D-related endpoints (projects, tasks, positions)
```

#### Step 3: Copy Integration Code
See **`API_INTEGRATION_GUIDE.md`** → Unity section for:
- UnityWebRequest setup
- ApiClient singleton
- AuthManager
- SignalR connection

#### Step 4: Create Data Models
```csharp
[System.Serializable]
public class Project
{
    public string id;
    public string name;
    public string description;
    public string repositoryUrl;
}
```

#### Step 5: Build 3D Experience! 🎉
```csharp
// Example: Load projects into 3D scene
StartCoroutine(ApiClient.Instance.Get<Project[]>(
    "/api/employee/projects",
    (projects) => {
        foreach (var project in projects) {
            CreateProjectBoard(project);
        }
    },
    (error) => Debug.LogError(error)
));
```

---

## 🔑 Authentication Flow (All Teams)

### 1. Register New User
```http
POST /api/auth/register
{
  "fullName": "Test User",
  "email": "test@example.com",
  "password": "Password@123",
  "confirmPassword": "Password@123",
  "phoneNumber": "+201234567890",
  "role": "Employee"  // or "Manager"
}
```
**Response:** `{ "data": { "userId": "..." } }`

### 2. Verify Email (OTP)
```http
POST /api/auth/verify-email/{userId}
{
  "otp": "123456"  // From email
}
```

### 3. Login
```http
POST /api/auth/login
{
  "email": "test@example.com",
  "password": "Password@123"
}
```
**Response:** `{ "data": { "token": "eyJ...", ... } }`

### 4. Save Token
```javascript
// Web
localStorage.setItem('jwt_token', token);

// Flutter
await _storage.write(key: 'jwt_token', value: token);

// Unity
PlayerPrefs.SetString("jwt_token", token);
```

### 5. Use Token in Requests
```http
Authorization: Bearer {jwt_token}
```

---

## 🏗️ Basic Integration Example

### Example: Display User's Projects

#### React
```typescript
import { useEffect, useState } from 'react';
import { apiClient } from './api-client';

function ProjectsList() {
  const [projects, setProjects] = useState([]);

  useEffect(() => {
    apiClient.get('/api/employee/projects')
      .then(response => setProjects(response.data))
      .catch(error => console.error(error));
  }, []);

  return (
    <div>
      {projects.map(project => (
        <div key={project.id}>
          <h3>{project.name}</h3>
          <p>{project.description}</p>
        </div>
      ))}
    </div>
  );
}
```

#### Flutter
```dart
class ProjectsList extends StatefulWidget {
  @override
  _ProjectsListState createState() => _ProjectsListState();
}

class _ProjectsListState extends State<ProjectsList> {
  List<Project> projects = [];

  @override
  void initState() {
    super.initState();
    _loadProjects();
  }

  Future<void> _loadProjects() async {
    final response = await apiClient.get('/api/employee/projects');
    setState(() {
      projects = (response['data'] as List)
          .map((json) => Project.fromJson(json))
          .toList();
    });
  }

  @override
  Widget build(BuildContext context) {
    return ListView.builder(
      itemCount: projects.length,
      itemBuilder: (context, index) {
        final project = projects[index];
        return ListTile(
          title: Text(project.name),
          subtitle: Text(project.description),
        );
      },
    );
  }
}
```

#### Unity
```csharp
public class ProjectsManager : MonoBehaviour
{
    void Start()
    {
        LoadProjects();
    }

    void LoadProjects()
    {
        StartCoroutine(ApiClient.Instance.Get<Project[]>(
            "/api/employee/projects",
            (projects) => {
                foreach (var project in projects) {
                    Debug.Log($"Project: {project.name}");
                    // Create 3D representation
                    GameObject projectBoard = CreateProjectBoard(project);
                }
            },
            (error) => {
                Debug.LogError($"Failed to load projects: {error}");
            }
        ));
    }

    GameObject CreateProjectBoard(Project project)
    {
        // Create 3D board in virtual office
        GameObject board = Instantiate(projectBoardPrefab);
        board.GetComponent<ProjectBoard>().SetProject(project);
        return board;
    }
}
```

---

## 🔔 Real-time Notifications (Optional)

### When to Use SignalR
- ✅ Task assignments/updates
- ✅ Live chat/comments
- ✅ 3D position tracking
- ✅ Dashboard live updates
- ❌ Historical data (use REST API)

### Quick Setup

#### JavaScript
```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5131/hubs/notifications", {
        accessTokenFactory: () => localStorage.getItem('jwt_token')
    })
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveNotification", (notification) => {
    console.log("New notification:", notification);
    showToast(notification.title, notification.message);
});

await connection.start();
```

See **`SIGNALR_INTEGRATION_GUIDE.md`** for complete examples.

---

## 📊 Common API Endpoints

### Authentication
```
POST   /api/auth/register
POST   /api/auth/verify-email/{userId}
POST   /api/auth/login
POST   /api/auth/forgot-password
```

### Projects (Employee)
```
GET    /api/employee/projects         # My projects
GET    /api/employee/projects/{id}     # Project details
GET    /api/employee/invitations       # My invitations
POST   /api/employee/invitations/{id}/respond
```

### Projects (Manager)
```
POST   /api/projects                   # Create
GET    /api/projects/{id}              # Get
PUT    /api/projects/{id}              # Update
DELETE /api/projects/{id}              # Delete
POST   /api/projects/{id}/invite       # Invite employee
```

### Tasks (Employee)
```
GET    /api/employee/tasks/my          # My tasks
PUT    /api/employee/tasks/{id}/start  # Start task
PUT    /api/employee/tasks/{id}/submit # Submit task
```

### Tasks (Manager)
```
POST   /api/tasks                      # Create task
PUT    /api/tasks/{id}                 # Update task
PUT    /api/tasks/{id}/confirm         # Approve task
PUT    /api/tasks/{id}/reject          # Reject task
```

### File Attachments
```
POST   /api/project-attachments/upload
GET    /api/project-attachments/{projectId}
GET    /api/project-attachments/download/{id}
POST   /api/task-attachments/upload
GET    /api/task-attachments/{taskId}
```

### Notifications
```
GET    /api/notifications              # Get all
GET    /api/notifications/unread-count
PUT    /api/notifications/{id}/read
PUT    /api/notifications/mark-all-read
```

See **Postman Collection** for all 68 endpoints!

---

## 🔧 Development Setup

### API Server Info
```
Base URL (HTTP):  http://localhost:5131
Base URL (HTTPS): https://localhost:7001
Swagger UI:       http://localhost:5131/swagger
SignalR Hub:      ws://localhost:5131/hubs/notifications
```

### CORS
CORS is enabled for all origins in development:
```
- http://localhost:3000  (React)
- http://localhost:4200  (Angular)
- http://localhost:8080  (Vue)
- http://localhost:5173  (Vite)
```

### Test Accounts
After running the API, a default admin account is created:
```
Email: admin@syncverse.com
Password: Admin@123
Role: Manager
```

---

## ✅ Integration Checklist

### All Teams
- [ ] Import Postman collection
- [ ] Test authentication flow in Postman
- [ ] Setup HTTP client in your framework
- [ ] Implement token storage
- [ ] Add Authorization header to requests
- [ ] Test error handling
- [ ] Setup environment variables

### Frontend Teams (Additional)
- [ ] Setup SignalR connection
- [ ] Implement loading states
- [ ] Add error toast notifications
- [ ] Setup data caching (React Query/SWR)
- [ ] Handle token expiration

### Flutter Team (Additional)
- [ ] Setup Dio interceptors
- [ ] Use secure storage for tokens
- [ ] Implement offline support
- [ ] Add local notifications for SignalR
- [ ] Handle network connectivity

### Gaming Team (Additional)
- [ ] Setup UnityWebRequest wrapper
- [ ] Create data models for all entities
- [ ] Implement SignalR for 3D position tracking
- [ ] Handle coroutines properly
- [ ] Add loading UI for API calls

---

## 🆘 Troubleshooting

### Common Issues

#### "CORS Error" (Web)
**Solution:** Make sure API is running on http://localhost:5131

#### "Unauthorized" Error
**Solution:** Check JWT token is present and valid

#### "Cannot connect to SignalR"
**Solution:** Verify token is passed in query string: `?access_token={token}`

#### "Network Error" (Mobile)
**Solution:** 
- Android: Use `10.0.2.2` instead of `localhost`
- iOS: Enable "Allow Arbitrary Loads" in Info.plist for development

---

## 📞 Need Help?

### Documentation Files
1. **API_INTEGRATION_GUIDE.md** - Detailed code examples
2. **SIGNALR_INTEGRATION_GUIDE.md** - Real-time setup
3. **ERROR_CODES_DOCUMENTATION.md** - Error handling
4. **POSTMAN_COLLECTION_README.md** - Postman guide

### Resources
- Swagger UI: http://localhost:5131/swagger
- Postman Collection: Test all endpoints
- Backend Team: Contact for API issues

---

## 🎉 You're Ready to Build!

1. **Start with Authentication** - Get login working first
2. **Test with Postman** - Before writing code
3. **Use the Guides** - Copy example code
4. **Ask Questions** - Backend team is here to help

**Good luck! 🚀**

---

**Last Updated:** March 2024
