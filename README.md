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

This project follows **Clean Architecture** principles with clear separation of concerns:
┌─────────────────────────────────────────────────────┐ │                   API Layer                         │ │  • REST Controllers                                 │ │  • SignalR Hubs (Real-time Communication)          │ │  • JWT Authentication Middleware                   │ │  • Exception Handling Middleware                   │ └────────────────────┬────────────────────────────────┘ │ depends on ┌────────────────────▼────────────────────────────────┐ │              Application Layer                      │ │  • Business Logic Services                         │ │  • DTOs & Mapping Profiles                         │ │  • Interfaces (Contracts)                          │ │  • Validation Rules                                │ └────────────────────┬────────────────────────────────┘ │ depends on ┌────────────────────▼────────────────────────────────┐ │                Domain Layer                         │ │  • Entities (User, Project, Task, etc.)           │ │  • Enums (TaskStatus, ProjectRole, etc.)          │ │  • Business Rules                                  │ │  • Domain Events                                   │ └─────────────────────────────────────────────────────┘ ▲ │ implements ┌────────────────────┴────────────────────────────────┐ │            Infrastructure Layer                     │ │  • Entity Framework Core (Repositories)            │ │  • Database Context & Migrations                   │ │  • SignalR Real-time Service                       │ │  • Identity Configuration                          │ └─────────────────────────────────────────────────────┘

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
| **Authentication** | ASP.NET Core Identity, JWT |
| **Real-time** | SignalR |
| **Mapping** | AutoMapper |
| **Validation** | FluentValidation |
| **API Documentation** | Swagger/OpenAPI |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB or full instance)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository**
   git clone https://github.com/syncverse12/Backend.Net.git cd Backend.Net

2. **Update connection string**

   Edit `appsettings.json`:
   { "ConnectionStrings": { "Default": "Server=(localdb)\mssqllocaldb;Database=ProjectManagementDB;Trusted_Connection=true;MultipleActiveResultSets=true" } }

3. **Update JWT settings**
   
   Edit `appsettings.json`:
   { "JwtSettings": { "securityKey": "your-secret-key-min-32-characters", "validIssuer": "https://localhost:7001", "validAudience": "https://localhost:7001", "expiryInMinutes": 60 } }

4. **Apply database migrations**
   dotnet ef database update

5. **Run the application**
   dotnet run --project API

6. **Access Swagger UI**
   https://localhost:7001/swagger

### Default Admin Credentials

After first run, a default admin account is created:
- **Email**: `admin@example.com`
- **Password**: `Admin@123`

⚠️ **Important**: Change these credentials immediately in production!

---

## 📚 API Documentation

### Authentication Endpoints
POST /api/auth/register Content-Type: application/json
{ "email": "user@example.com", "password": "Password@123", "firstName": "John", "lastName": "Doe" }

### Task Management Endpoints

#### Create Task (Manager/Team Leader)

#### Start Task (Employee)

#### Submit Task (Employee)

### SignalR Real-time Connection

**Hub URL**: `/hubs/notifications`

**Client Example** (JavaScript/TypeScript):

For complete API documentation, visit `/swagger` after running the application.

---

## 📁 Project Structure
Backend.Net/ │ ├── API/                                    # Presentation Layer │   ├── Controllers/ │   │   ├── Tasks/Manager/ │   │   ├── project/Manager/ │   │   └── project/Employee/ │   ├── Hubs/ │   │   └── NotificationHub.cs           # SignalR Hub │   ├── Middleware/ │   │   └── ExceptionMiddleware.cs │   └── Program.cs │ ├── Application/                            # Business Logic Layer │   ├── Interfaces/ │   │   ├── Notifications/ │   │   │   ├── INotificationService.cs │   │   │   └── IRealtimeNotificationService.cs │   │   ├── Tasks/ │   │   ├── Project/ │   │   └── Identity/ │   ├── Services/ │   │   ├── Notifications/ │   │   │   └── NotificationService.cs │   │   ├── Tasks/ │   │   │   ├── Manager/TaskService.cs │   │   │   └── Employee/EmployeeTaskService.cs │   │   ├── Project/ │   │   └── Identity/ │   ├── DTOs/ │   └── Mapping/ │ ├── Domain/                                 # Core Domain Layer │   ├── Entities/ │   │   ├── User.cs │   │   ├── Project.cs │   │   ├── TaskItem.cs │   │   ├── Notification.cs │   │   └── ProjectMember.cs │   └── Enums/ │       ├── TaskStatus.cs │       ├── ProjectRole.cs │       └── NotificationType.cs │ └── Infrastructure/                         # External Concerns Layer ├── Data/ │   └── DatabaseDbContext.cs ├── Persistence/ │   └── Repositories/ │       ├── GenericRepository.cs │       └── UnitOfWork.cs └── Realtime/ └── SignalRNotificationService.cs  # SignalR Implementation

---

## 🗃️ Database Schema

### Core Entities

- **User** - System users with roles
- **Workspace** - Top-level container
- **Project** - Projects within workspaces
- **ProjectMember** - Team membership with roles
- **Milestone** - Project phases
- **TaskItem** - Individual tasks
- **TaskDependency** - Task relationships
- **Category** - Task categories
- **TimeLog** - Time tracking
- **Notification** - System notifications
- **ProjectInvitation** - Membership invitations

### Relationships
User 1──────* ProjectMember ──────1 Project Project 1─────── Milestone Milestone 1─────* TaskItem TaskItem 1──────* TaskDependency TaskItem 1──────* TimeLog User 1──────────* Notification

---

## 🎯 Business Rules

### Task Status Transitions
✅ Valid Transitions: Pending → InProgress InProgress → Submitted | Pending Submitted → Completed | Rejected Rejected → InProgress
❌ Invalid Transitions: Pending → Completed (skip workflow) Completed → Any (immutable)

### Task Dependencies
✅ Valid:
•	Tasks in same project
•	No self-dependencies
•	No circular dependencies
❌ Invalid:
•	Task depends on itself
•	Circular: A→B→C→A
•	Cross-project dependencies

### Authorization Rules
Workspace Owner: ✅ Full control over workspace and projects
Project Manager: ✅ Manage project, milestones, tasks ✅ Invite team leaders and members
Team Leader: ✅ Create and assign tasks ✅ Review submissions ✅ Invite team members
Team Member: ✅ Work on assigned tasks only ✅ Submit work for review

---

## 🔐 Security Features

### Authentication
- ✅ JWT token-based (Bearer authentication)
- ✅ Secure password hashing (PBKDF2)
- ✅ Token expiration and refresh
- ✅ SignalR authentication via access_token query param

### Authorization
- ✅ Role-based access control (RBAC)
- ✅ Resource-based authorization (project members only)
- ✅ Custom authorization handlers
- ✅ Policy-based authorization

### Data Protection
- ✅ Soft delete (no data loss)
- ✅ Audit trails (CreatedBy, UpdatedAt)
- ✅ Input validation (FluentValidation)
- ✅ SQL injection protection (EF Core)
- ✅ XSS protection

---

## 🧪 Testing

### Unit Test Example (using xUnit)

---

## 📈 Performance Considerations

### Optimizations

- ✅ **Pagination** - All list endpoints support paging
- ✅ **Eager Loading** - Minimize database round-trips
- ✅ **AsNoTracking** - Read-only queries optimization
- ✅ **Indexed Columns** - Fast lookups on frequently queried fields
- ✅ **DTO Projection** - Transfer only required data

### Scalability

- ✅ **Repository Pattern** - Database abstraction
- ✅ **Unit of Work** - Transaction management
- ✅ **SignalR Scaleout** - Ready for Redis/Azure SignalR
- ✅ **Async/Await** - Non-blocking operations

---

## 🛡️ Error Handling

### Centralized Exception Middleware

---

## 📖 Documentation

- **API Documentation**: Available at `/swagger` endpoint
- **Architecture Diagrams**: See [Architecture](#-architecture) section
- **Code Comments**: Comprehensive inline documentation
- **XML Documentation**: Enabled for all public APIs

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Code Style

- Follow Clean Architecture principles
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Write unit tests for business logic
- Follow C# naming conventions

---

## 📊 Project Statistics

| Metric | Count |
|--------|-------|
| API Endpoints | 40+ |
| Core Entities | 12+ |
| Services | 10+ |
| DTOs | 25+ |
| Business Validations | 30+ |
| Notification Types | 10+ |
| Unit Tests | TBD |

---

## 🎓 Learning Resources

This project demonstrates:

- ✅ Clean Architecture implementation in .NET 8
- ✅ SOLID principles in practice
- ✅ Domain-Driven Design (DDD)
- ✅ Repository and Unit of Work patterns
- ✅ SignalR real-time communication
- ✅ JWT authentication and authorization
- ✅ Graph algorithms (DFS for circular dependencies)
- ✅ State machine pattern (task workflow)

---

## 🔮 Roadmap

### Planned Features

- [ ] Email notifications integration
- [ ] File attachments for tasks
- [ ] Task comments and discussions
- [ ] Gantt chart visualization
- [ ] Sprint/Agile board management
- [ ] Burndown charts
- [ ] Export to PDF/Excel
- [ ] Task templates
- [ ] Recurring tasks
- [ ] Mobile app (React Native/Flutter)
- [ ] Docker containerization
- [ ] CI/CD pipeline

---

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👤 Author

**Your Name**  
📧 Email: your.email@example.com  
🔗 LinkedIn: [Your LinkedIn](https://linkedin.com/in/yourprofile)  
🐙 GitHub: [@syncverse12](https://github.com/syncverse12)

---

## 🙏 Acknowledgments

- Inspired by Clean Architecture principles by Robert C. Martin
- Built with modern .NET 8 and C# 12 features
- SignalR for real-time communication
- Entity Framework Core for data access

---

## 📞 Support

For issues, questions, or suggestions:

- 🐛 [Open an Issue](https://github.com/syncverse12/Backend.Net/issues)
- 💬 [Start a Discussion](https://github.com/syncverse12/Backend.Net/discussions)
- 📧 Email: your.email@example.com

---

## ⭐ Star This Project

If you find this project useful, please consider giving it a ⭐ on GitHub!

---

**Built using .NET 8 and Clean Architecture**