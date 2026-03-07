# 🎯 Project Management System

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat&logo=c-sharp)
![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Build](https://img.shields.io/badge/build-passing-brightgreen)

A **production-ready**, **enterprise-level** project management system built with **.NET 8** following **Clean Architecture** principles. Features include real-time notifications, advanced task dependency management, time tracking, and comprehensive team collaboration tools.

---

## 📑 Table of Contents

- [Features](#-features)
- [Architecture](#-architecture)
- [Technology Stack](#-technology-stack)
- [Getting Started](#-getting-started)
- [API Documentation](#-api-documentation)
- [Project Structure](#-project-structure)
- [Key Highlights](#-key-highlights)
- [Contributing](#-contributing)
- [License](#-license)

---

## ✨ Features

### 🔐 Authentication & Authorization
- JWT-based authentication with secure token management
- Role-based access control (Admin, Manager, Employee)
- Resource-based authorization
- Custom authorization policies and handlers

### 📊 Project Management
- Multi-project workspace organization
- Hierarchical project structure (Workspace → Project → Milestone → Task)
- Team member management with role assignments
- Soft delete with restore capabilities
- Project invitations and collaboration

### 🏢 Workspace Management
- Create and manage multiple workspaces
- Workspace-level access control and ownership
- Project organization within workspaces
- Soft delete with restore capabilities

### 🎯 Milestone Management
- Create milestones within projects (sprints/phases)
- Define start/end dates for project timelines
- Link tasks to specific milestones
- Track milestone progress and completion
- Date validation for task due dates within milestone bounds

### 🏷️ Category Management
- Create custom task categories (Frontend, Backend, DevOps, etc.)
- Assign categories to tasks for organization
- Category-based analytics and reporting
- Soft delete support with cascade handling

### 💬 Task Comments System
- Add comments to any task
- Comment history tracking with timestamps
- Automatic notifications to task participants (assignee + creator)
- Edit/delete own comments
- Rich collaboration features

### 👥 Team Management
- Invite team members with role-based assignments
- Role management: Project Manager, Team Leader, Team Member
- Team specializations: Frontend, Backend, FullStack, DevOps, QA, Design, Mobile
- Update member roles dynamically
- Remove team members with proper authorization

### ✅ Advanced Task Management
- Complete task lifecycle workflow (Pending → InProgress → Submitted → Completed)
- **Circular dependency detection** using DFS algorithm
- Smart task assignment with automatic notifications
- Task review system (Accept/Reject with comments)
- Advanced filtering, sorting, and search
- Task categorization and analytics

### 🔔 Real-time Notifications (SignalR)
- **Instant push notifications** for all task and project events
- User-specific notification groups
- Clean Architecture implementation with dependency inversion
- Mark as read/unread functionality
- Notification history and management

### ⏱️ Time Tracking
- Start/stop timer for tasks
- Automatic time calculation
- Auto-stop timer on task submission
- Time analytics and reporting

### 📈 Analytics & Dashboards
- Task status statistics
- Employee performance metrics
- Category-based analytics
- Project completion tracking
- Overdue task monitoring

---

## 🏗️ Architecture

This project follows **Clean Architecture** principles with clear separation of concerns

┌─────────────────────────────────────────┐ │         Presentation Layer (API)        │ │  Controllers, Hubs, Middleware          │ └─────────────────┬───────────────────────┘ │ ┌─────────────────▼───────────────────────┐ │        Application Layer                │ │  Services, DTOs, Interfaces, Validators │ └─────────────────┬───────────────────────┘ │ ┌─────────────────▼───────────────────────┐ │           Domain Layer                  │ │  Entities, Value Objects, Domain Logic  │ └─────────────────┬───────────────────────┘ │ ┌─────────────────▼───────────────────────┐ │       Infrastructure Layer              │ │  DbContext, Repositories, External APIs │ └─────────────────────────────────────────┘

**Benefits:** Testable, Flexible, No layer violations, SOLID principles

### Key Architecture Benefits

✅ **Dependency Inversion** - Inner layers don't depend on outer layers  
✅ **Testability** - Easy to mock and unit test  
✅ **Maintainability** - Clear separation of concerns  
✅ **Scalability** - Easy to extend and modify  
✅ **Flexibility** - Swap implementations without changing business logic

---

## 🛠️ Technology Stack

| Category | Technologies |
|----------|-------------|
| **Framework** | .NET 8 |
| **Language** | C# 12.0 |
| **ORM** | Entity Framework Core 8 |
| **Database** | SQL Server |
| **Authentication** | ASP.NET Core Identity, JWT Bearer |
| **Real-time** | SignalR (WebSocket) |
| **Mapping** | AutoMapper |
| **Validation** | FluentValidation |
| **API Documentation** | Swagger/OpenAPI 3.0 |
| **Patterns** | Repository, Unit of Work, Result, CQRS |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB or full instance)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository**

2. **Update connection string**

3. **Update JWT settings**

4. **Apply database migrations**

5. **Run the application**

6. **Access Swagger UI**

### Default Admin Credentials

After first run, a default admin account is created:
- **Email**: `admin@example.com`
- **Password**: `Admin@123`

⚠️ **Important**: Change these credentials immediately in production!

---

## 📚 API Documentation

### 🔐 Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Register new user | ❌ |
| POST | `/api/auth/login` | Login and get JWT token | ❌ |
| GET | `/api/users` | List all users | ✅ Admin |
| PUT | `/api/users/{id}/role` | Update user role | ✅ Admin |

**Example Request:**
````````
POST /api/auth/register
Content-Type: application/json

{
  "email": "newuser@example.com",
  "password": "User@123",
  "role": "Employee"
}
````````

**Example Response:**
````````
HTTP/1.1 201 Created
Location: /api/auth/login

{
  "token": "jwt.token.here",
  "expiresIn": 3600
}
````````

---

### 🏢 Workspaces

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/workspaces` | Create workspace | Manager |
| GET | `/api/workspaces` | Get my workspaces | User |
| GET | `/api/workspaces/{id}` | Get workspace details | Owner/Member |
| PUT | `/api/workspaces/{id}` | Update workspace | Owner |
| DELETE | `/api/workspaces/{id}` | Delete workspace | Owner |
| PUT | `/api/workspaces/{id}/restore` | Restore workspace | Owner |

**Example Request:**
````````
POST /api/workspaces
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "New Workspace",
  "description": "Workspace for project X"
}
````````

**Example Response:**
````````
HTTP/1.1 201 Created
Location: /api/workspaces/1

{
  "id": 1,
  "name": "New Workspace",
  "description": "Workspace for project X",
  "ownerId": "user-id",
  "members": [],
  "projects": []
}
````````

---

### 📊 Projects

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/projects` | Create project | Owner/PM |
| GET | `/api/projects/{id}` | Get project details | Member |
| PUT | `/api/projects/{id}` | Update project | Owner/PM |
| DELETE | `/api/projects/{id}` | Soft delete project | Owner/PM |
| PUT | `/api/projects/{id}/restore` | Restore project | Owner/PM |
| GET | `/api/projects/workspace/{id}` | Get workspace projects | Owner |
| POST | `/api/projects/{id}/invite` | Invite employee | Owner/PM |

**Example Request:**
````````
POST /api/projects
Authorization: Bearer {token}
Content-Type: application/json

{
  "workspaceId": 1,
  "name": "New Project",
  "description": "Project for feature Y",
  "startDate": "2023-10-01",
  "endDate": "2023-10-31"
}
````````

**Example Response:**
````````
HTTP/1.1 201 Created
Location: /api/projects/1

{
  "id": 1,
  "name": "New Project",
  "description": "Project for feature Y",
  "startDate": "2023-10-01T00:00:00",
  "endDate": "2023-10-31T23:59:59",
  "workspaceId": 1,
  "tasks": [],
  "milestones": []
}
````````

---

### 🎯 Milestones

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/milestones` | Create milestone | Owner/PM |
| GET | `/api/milestones/project/{id}` | Get project milestones | Member |
| GET | `/api/milestones/{id}` | Get milestone details | Member |
| PUT | `/api/milestones/{id}` | Update milestone | Owner/PM |
| DELETE | `/api/milestones/{id}` | Delete milestone | Owner/PM |
| PUT | `/api/milestones/{id}/restore` | Restore milestone | Owner/PM |

**Example Request:**
````````
POST /api/milestones
Authorization: Bearer {token}
Content-Type: application/json

{
  "projectId": 1,
  "name": "Milestone 1",
  "description": "First milestone for project",
  "dueDate": "2023-10-15"
}
````````

**Example Response:**
````````
HTTP/1.1 201 Created
Location: /api/milestones/1

{
  "id": 1,
  "name": "Milestone 1",
  "description": "First milestone for project",
  "dueDate": "2023-10-15T23:59:59",
  "projectId": 1,
  "tasks": []
}
````````

---

### ✅ Tasks (Manager/Team Leader)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/tasks` | Create task | Owner/PM/TL |
| GET | `/api/tasks/manager/tasks` | List tasks (paginated) | PM/TL |
| GET | `/api/tasks/{id}` | Get task details | Member |
| PUT | `/api/tasks/{id}` | Update task | Owner/PM/TL |
| DELETE | `/api/tasks/{id}` | Soft delete task | Owner/PM/TL |
| PUT | `/api/tasks/{id}/restore` | Restore task | Owner/PM/TL |
| POST | `/api/tasks/dependency` | Add dependency | Owner/PM/TL |
| PUT | `/api/tasks/{id}/confirm` | Approve task | PM/TL |
| PUT | `/api/tasks/{id}/reject` | Reject task | PM/TL |
| GET | `/api/tasks/manager/dashboard` | Analytics dashboard | PM/TL |
| POST | `/api/tasks/filter` | Advanced filtering | PM/TL |
| GET | `/api/tasks/dashboard/{projectId}` | Project dashboard | PM/TL |

**Example Request:**
````````
POST /api/tasks
Authorization: Bearer {token}
Content-Type: application/json

{
  "projectId": 1,
  "name": "New Task",
  "description": "Task details here",
  "assignedTo": "user-id",
  "dueDate": "2023-10-10",
  "priority": "High"
}
````````

**Example Response:**
````````
HTTP/1.1 201 Created
Location: /api/tasks/1

{
  "id": 1,
  "name": "New Task",
  "description": "Task details here",
  "assignedTo": "user-id",
  "dueDate": "2023-10-10T23:59:59",
  "priority": "High",
  "projectId": 1,
  "dependencies": [],
  "comments": []
}
````````

---

### 👤 Tasks (Employee)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/employee/tasks` | My assigned tasks | Employee |
| GET | `/api/employee/tasks/{id}` | Task details | Employee |
| POST | `/api/employee/tasks/{id}/start` | Start working | Employee |
| POST | `/api/employee/tasks/{id}/submit` | Submit for review | Employee |
| GET | `/api/employee/tasks/blocked` | Blocked tasks | Employee |

**Example Request:**
````````
GET /api/employee/tasks
Authorization: Bearer {token}
````````

**Example Response:**
````````
HTTP/1.1 200 OK

{
  "tasks": [
    {
      "id": 1,
      "name": "Task 1",
      "description": "Details about task 1",
      "status": "Pending",
      "priority": "Medium",
      "dueDate": "2023-10-05",
      "createdDate": "2023-09-01"
    },
    {
      "id": 2,
      "name": "Task 2",
      "description": "Details about task 2",
      "status": "InProgress",
      "priority": "High",
      "dueDate": "2023-10-10",
      "createdDate": "2023-09-15"
    }
  ]
}
````````

---

### 🏷️ Categories

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/categories` | Create category | PM |
| GET | `/api/categories` | List categories | User |
| GET | `/api/categories/{id}` | Get category details | User |
| PUT | `/api/categories/{id}` | Update category | PM |
| DELETE | `/api/categories/{id}` | Delete category | PM |

**Example Request:**
````````
POST /api/categories
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Frontend",
  "description": "Tasks related to frontend development"
}
````````

**Example Response:**
````````
HTTP/1.1 201 Created
Location: /api/categories/1

{
  "id": 1,
  "name": "Frontend",
  "description": "Tasks related to frontend development",
  "tasks": []
}
````````

---

### 💬 Task Comments

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/tasks/{id}/comments` | Add comment | Member |
| GET | `/api/tasks/{id}/comments` | Get comments | Member |
| PUT | `/api/comments/{id}` | Update comment | Author |
| DELETE | `/api/comments/{id}` | Delete comment | Author |

**Example Request:**
````````
POST /api/tasks/1/comments
Authorization: Bearer {token}
Content-Type: application/json

{
  "text": "This is a comment on the task"
}
````````

**Example Response:**
````````
HTTP/1.1 201 Created
Location: /api/comments/1

{
  "id": 1,
  "taskId": 1,
  "authorId": "user-id",
  "text": "This is a comment on the task",
  "createdDate": "2023-09-20T10:30:00"
}
````````

---

### 👥 Team Management

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/teams/project/{id}` | Get team members | Member |
| POST | `/api/teams/invite` | Invite member | Owner/PM/TL |
| PUT | `/api/teams/member/role` | Update role | Owner/PM |
| DELETE | `/api/teams/member/{id}` | Remove member | Owner/PM |

**Example Request:**
````````
POST /api/teams/invite
Authorization: Bearer {token}
Content-Type: application/json

{
  "projectId": 1,
  "email": "newmember@example.com",
  "role": "Developer"
}
````````

**Example Response:**
````````
HTTP/1.1 201 Created
Location: /api/teams/project/1

{
  "projectId": 1,
  "teamMembers": [
    {
      "id": "user-id",
      "email": "existingmember@example.com",
      "role": "Developer"
    },
    {
      "id": "new-member-id",
      "email": "newmember@example.com",
      "role": "Developer"
    }
  ]
}
````````

---

### ⏱️ Time Tracking

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/timelogs/start` | Start timer | Employee |
| PUT | `/api/timelogs/stop` | Stop timer | Employee |
| GET | `/api/timelogs/task/{taskId}` | Get task time logs | Member |
| GET | `/api/timelogs/my-logs` | My time logs | Employee |
| DELETE | `/api/timelogs/{id}` | Delete time log | Employee |

**Example Request:**
````````
POST /api/timelogs/start
Authorization: Bearer {token}
Content-Type: application/json

{
  "taskId": 1
}
````````

**Example Response:**
````````
HTTP/1.1 201 Created
Location: /api/timelogs/1

{
  "id": 1,
  "taskId": 1,
  "userId": "user-id",
  "startTime": "2023-09-20T10:00:00",
  "endTime": null,
  "duration": 0
}
````````

---

### 🔔 Notifications

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/notifications` | Get my notifications | User |
| GET | `/api/notifications/unread-count` | Unread count | User |
| PUT | `/api/notifications/{id}/read` | Mark as read | User |
| PUT | `/api/notifications/read-all` | Mark all as read | User |
| DELETE | `/api/notifications/{id}` | Delete notification | User |

**SignalR Hub Endpoint**: `/hubs/notifications`

**SignalR Client Example** (JavaScript/TypeScript):
````````
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notifications")
    .build();

connection.on("ReceiveNotification", (notification) => {
    console.log("New notification:", notification);
    // Update UI or show toast
});

connection.start()
    .then(() => console.log("SignalR connected"))
    .catch(err => console.error("SignalR connection error:", err));
````````

---

## 📁 Project Structure

### 🎨 Layer Overview

| Layer | Location | Responsibility | Dependencies |
|-------|----------|----------------|--------------|
| **API** | `API/` | Presentation & Entry Point | Application |
| **Application** | `Application/` | Business Logic | Domain |
| **Domain** | `Domain/` | Core Entities & Rules | None (Pure) |
| **Infrastructure** | `Infrastructure/` | Technical Implementations | Application, Domain |

---

### Detailed Structure

<details>
<summary>📦 <b>API Layer</b> - Click to expand</summary>

- **`API/`**: Contains the presentation layer and entry point for the application.
  - **`Controllers/`**: API controllers for handling HTTP requests.
  - **`DTOs/`**: Data Transfer Objects for API communication.
  - **`Models/`**: View models and data models for the API.
  - **`Extensions/`**: Extension methods for API functionality.
  - **`Filters/`**: Action filters for logging, validation, etc.
  - **`Middlware/`**: Custom middleware for request processing.
  - **`Swagger/`**: Swagger configuration and setup.

</details>

<details>
<summary>📦 <b>Application Layer</b> - Click to expand</summary>

- **`Application/`**: Contains the business logic and application rules.
  - **`Services/`**: Application services for handling business operations.
  - **`Interfaces/`**: Interfaces for services, repositories, etc.
  - **`Specifications/`**: Business logic specifications and validations.
  - **`Mappings/`**: AutoMapper profiles for object mapping.

</details>

<details>
<summary>📦 <b>Domain Layer</b> - Click to expand</summary>

- **`Domain/`**: Contains the core entities and domain logic.
  - **`Entities/`**: Core business entities and their configurations.
  - **`ValueObjects/`**: Immutable value objects for domain consistency.
  - **`Enums/`**: Enumeration types used in the domain.
  - **`Events/`**: Domain events for event sourcing and messaging.

</details>

<details>
<summary>📦 <b>Infrastructure Layer</b> - Click to expand</summary>

- **`Infrastructure/`**: Contains technical implementations and external integrations.
  - **`Persistence/`**: Entity Framework Core DbContext and migrations.
  - **`Repositories/`**: Repository implementations for data access.
  - **`Services/`**: External services and integrations (e.g., email, SMS).
  - **`Logging/`**: Logging configuration and implementations.
  - **`Security/`**: Security features like JWT authentication and authorization.
  - **`SignalR/`**: SignalR hubs and configurations for real-time notifications.
  - **`Swagger/`**: Swagger UI setup and configuration.

</details>

---

## 🌟 Key Highlights

### 1. Real-time Notifications with Clean Architecture

**Architecture Pattern:**
````````

**Notification Events:**

| Event | Triggered When | Recipients | Real-time? |
|-------|----------------|-----------|------------|
| Task Assigned | New task created & assigned | Assignee | ✅ |
| Task Reassigned | Task assigned to different user | Old + New | ✅ |
| Task Updated | Task details modified | Assignee | ✅ |
| Task Submitted | Employee submits work | Manager/TL | ✅ |
| Task Approved | Manager approves task | Employee | ✅ |
| Task Rejected | Manager rejects task | Employee | ✅ |
| Dependency Resolved | Prerequisite completed | Blocked assignee | ✅ |
| Comment Added | New comment on task | Participants | ✅ |
| Project Invitation | User invited to project | Invited user | ✅ |

---

### 2. Advanced Dependency Management

**Circular Detection Algorithm:**
- **Type**: Depth-First Search (DFS)
- **Complexity**: O(V + E) where V = tasks, E = dependencies
- **Detects**: Both direct and transitive circular dependencies

**Examples:**
````````

---

### 3. Permission Matrix

| Action | Workspace Owner | Project Manager | Team Leader | Team Member |
|--------|:---------------:|:---------------:|:-----------:|:-----------:|
| Create Workspace | ✅ | ❌ | ❌ | ❌ |
| Create Project | ✅ | ✅ | ❌ | ❌ |
| Create Milestones | ✅ | ✅ | ❌ | ❌ |
| Invite Team Leader | ✅ | ✅ | ❌ | ❌ |
| Invite Team Members | ✅ | ✅ | ✅ | ❌ |
| Create/Assign Tasks | ✅ | ✅ | ✅ | ❌ |
| Update/Delete Tasks | ✅ | ✅ | ✅ | ❌ |
| Review Tasks | ✅ | ✅ | ✅ | ❌ |
| Manage Dependencies | ✅ | ✅ | ✅ | ❌ |
| Start/Submit Tasks | ❌ | ❌ | ✅ | ✅ |
| Add Comments | ✅ | ✅ | ✅ | ✅ |
| Track Time | ❌ | ❌ | ✅ | ✅ |
| View Analytics | ✅ | ✅ | ✅ | ❌ |
````````

### 4. Metrics

| Metric | Count |
|--------|-------|
| **API Endpoints** | 50+ |
| **Core Entities** | 13 |
| **Services** | 12+ |
| **DTOs** | 30+ |
| **Business Validations** | 35+ |
| **Notification Types** | 10 |
| **Enum Types** | 8 |
| **Design Patterns** | 6+ |
````````

---

### ✅ Completed Features

- [x] Authentication & JWT
- [x] Workspace Management
- [x] Project Management
- [x] Milestone Management
- [x] Task Management with Dependencies
- [x] Circular Dependency Detection (DFS)
- [x] Real-time Notifications (SignalR)
- [x] Task Comments System
- [x] Category Management
- [x] Team Management with Roles
- [x] Time Tracking
- [x] Analytics Dashboards
- [x] Advanced Filtering & Search
- [x] Soft Delete & Restore
- [x] Role-based Authorization

### 🚧 Planned Features

- [ ] Email notifications integration
- [ ] File attachments for tasks
- [ ] Gantt chart visualization
- [ ] Sprint/Agile board (Kanban)
- [ ] Burndown charts
- [ ] Export to PDF/Excel
- [ ] Task templates
- [ ] Recurring tasks
- [ ] Mobile app
- [ ] Docker containerization
- [ ] CI/CD pipeline