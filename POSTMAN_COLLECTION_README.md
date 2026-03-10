# 📮 SyncVerse API - Postman Collection

## 📁 Files Created

1. **`SyncVerse_API_Collection.postman_collection.json`** - Complete API endpoints collection
2. **`SyncVerse_API_Environment.postman_environment.json`** - Development environment variables

---

## 🚀 How to Import into Postman

### 1️⃣ Import Collection
1. Open **Postman**
2. Click **Import** button (top left)
3. Select **`SyncVerse_API_Collection.postman_collection.json`**
4. Click **Import**

### 2️⃣ Import Environment
1. Click on **Environments** (left sidebar)
2. Click **Import**
3. Select **`SyncVerse_API_Environment.postman_environment.json`**
4. Click **Import**
5. Select **"SyncVerse - Development"** from the environment dropdown (top right)

---

## 📋 Collection Structure

### 🔐 Authentication (6 endpoints)
- Register
- Verify Email
- Login
- Forgot Password
- Verify Reset OTP
- Reset Password

### 🏢 Workspaces - Manager Only (6 endpoints)
- Create Workspace
- Get All Workspaces
- Get Workspace by ID
- Update Workspace
- Delete Workspace (Soft Delete)
- Restore Workspace

### 📁 Projects - Manager (7 endpoints)
- Create Project
- Get Project by ID
- Get Projects by Workspace
- Update Project
- Delete Project (Soft Delete)
- Restore Project
- Invite Employee to Project

### 👨‍💼 Employee - Projects (4 endpoints)
- Get My Projects
- Get Project Details
- Get My Invitations
- Respond to Invitation (Accept/Reject)

### 🎯 Milestones - Manager (6 endpoints)
- Create Milestone
- Get Milestone by ID
- Get Project Milestones
- Update Milestone
- Delete Milestone
- Restore Milestone

### ✅ Tasks - Manager (11 endpoints)
- Create Task
- Get Manager Tasks (with filters)
- Update Task
- Delete Task (Soft Delete)
- Restore Task
- Add Task Dependency
- Confirm Task (Approve)
- Reject Task
- Get Manager Dashboard
- Filter Tasks
- Get Project Dashboard

### 👨‍💼 Employee - Tasks (4 endpoints)
- Get My Tasks
- Get Task Details
- Start Task
- Submit Task

### 💬 Task Comments (4 endpoints)
- Add Comment
- Get Task Comments
- Update Comment
- Delete Comment

### ⏱️ Time Tracking (7 endpoints)
- Start Time Log
- Stop Time Log
- Create Manual Time Log
- Get Task Time Logs
- Get Active Working Time
- Get Total Time Spent
- Get My Time Logs

### 📎 File Attachments (8 endpoints)
- Upload Project Attachment
- Get Project Attachments
- Download Project Attachment
- Delete Project Attachment
- Upload Task Attachment
- Get Task Attachments
- Download Task Attachment
- Delete Task Attachment

### 🔔 Notifications (5 endpoints)
- Get My Notifications
- Get Unread Count
- Mark Notification as Read
- Mark All as Read
- Delete Notification

### 📂 Categories (3 endpoints)
- Create Category
- Get My Categories
- Delete Category

---

## 🔑 Authentication Flow

### For Manager:
```
1. Register → (Role: "Manager")
2. Verify Email → (Enter OTP)
3. Login → (JWT token saved automatically)
4. Use Manager endpoints
```

### For Employee:
```
1. Register → (Role: "Employee")
2. Verify Email → (Enter OTP)
3. Login → (JWT token saved automatically)
4. Wait for project invitation
5. Accept invitation
6. Access employee endpoints
```

---

## 🎯 Quick Start Testing Flow

### 1️⃣ Setup Account (Manager)
```
POST /api/auth/register
  → Get userId from response
POST /api/auth/verify-email/{userId}
POST /api/auth/login
  → JWT token saved automatically to environment
```

### 2️⃣ Create Workspace
```
POST /api/workspaces
  → workspaceId saved automatically
```

### 3️⃣ Create Project
```
POST /api/projects
  → projectId saved automatically
```

### 4️⃣ Create Milestone
```
POST /api/milestones
```

### 5️⃣ Create Task
```
POST /api/tasks
  → taskId saved automatically
```

### 6️⃣ Upload Files
```
POST /api/project-attachments/upload
POST /api/task-attachments/upload
```

### 7️⃣ Time Tracking
```
POST /api/tasks/{taskId}/timelogs/start
POST /api/tasks/{taskId}/timelogs/stop
```

---

## 🔧 Environment Variables

The following variables are **automatically set** by the collection scripts:

| Variable | Description | Auto-set by |
|----------|-------------|-------------|
| `jwt_token` | JWT authentication token | Login endpoint |
| `userId` | Current user ID | Register endpoint |
| `workspaceId` | Created workspace ID | Create Workspace |
| `projectId` | Created project ID | Create Project |
| `taskId` | Created task ID | Create Task |

You can also **manually set** these variables in the Environment tab.

---

## 📝 Notes

### Authorization
- Most endpoints require JWT authentication
- Token is automatically added to requests via Bearer Token
- **Manager-only endpoints** require Manager role
- **Employee-only endpoints** require Employee role

### Base URLs
- **HTTP**: `http://localhost:5131` (Development)
- **HTTPS**: `https://localhost:7001` (Development SSL)

### File Upload
- File upload endpoints use `multipart/form-data`
- Select file from your computer in Postman Body → form-data
- Supported on:
  - `/api/project-attachments/upload`
  - `/api/task-attachments/upload`

### SignalR Hub
- **Real-time notifications**: `ws://localhost:5131/hubs/notifications`
- Requires JWT token in query string: `?access_token={token}`

---

## 🎨 Features Highlights

### ✨ Current Features
- ✅ JWT Authentication with OTP verification
- ✅ Role-based authorization (Manager/Employee)
- ✅ Workspace management
- ✅ Project management with invitations
- ✅ Task management with dependencies
- ✅ Milestone tracking
- ✅ Time logging
- ✅ File attachments (Project & Task)
- ✅ Real-time notifications (SignalR)
- ✅ Comments system
- ✅ Soft delete & restore

### 🚀 Upcoming Features (As per roadmap)
- 🎮 3D Virtual Office Environment
- 🤖 AI Team Recommendations
- 📊 Advanced Analytics Dashboard
- 🎯 Kanban Board
- 📈 Gantt Charts

---

## 🐛 Troubleshooting

### Issue: "Unauthorized" errors
**Solution**: Make sure you're logged in and JWT token is set in environment

### Issue: "ManagerOnly policy failed"
**Solution**: Login with a Manager account, not Employee

### Issue: File upload fails
**Solution**: Ensure you're using `form-data` and selecting actual file

### Issue: Cannot find workspace/project/task
**Solution**: Check that IDs are correctly set in environment variables

---

## 📞 Support & Documentation

- **Swagger UI**: `http://localhost:5131/swagger` (when app is running)
- **GitHub**: https://github.com/syncverse12/Backend.Net
- **Database**: SQL Server (LocalDB or MSSQLSERVER)

---

## 🎯 Total Endpoints: **68 API Endpoints**

### By Category:
- Authentication: 6
- Workspaces: 6
- Projects (Manager): 7
- Projects (Employee): 4
- Milestones: 6
- Tasks (Manager): 11
- Tasks (Employee): 4
- Comments: 4
- Time Tracking: 7
- File Attachments: 8
- Notifications: 5
- Categories: 3

---

**Made with ❤️ for SyncVerse Project Management System**

*Last Updated: March 2024*
