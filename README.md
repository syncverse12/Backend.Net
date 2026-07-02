# 🎯 SyncVerse - Workspace & Project Management API

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat&logo=c-sharp)
![EF Core](https://img.shields.io/badge/EF_Core-8.0-33A2FF?style=flat&logo=nuget)
![SignalR](https://img.shields.io/badge/SignalR-RealTime-0078D4?style=flat)
![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue.svg)

**SyncVerse** is a production-ready, enterprise-level workspace and project management system built with **.NET 8** following **Clean Architecture** principles. It serves as a centralized hub for teams to collaborate, track time, manage tasks, and view insightful analytics across multiple roles.

---

## 📑 Table of Contents

- [Features](#-features)
  - [Core Modules](#core-modules)
  - [Role-Specific Dashboards](#role-specific-dashboards)
- [Architecture](#-architecture)
- [Technology Stack](#-technology-stack)
- [Getting Started](#-getting-started)
- [API Documentation](#-api-documentation)
- [Project Structure](#-project-structure)

---

## ✨ Features

### 🔐 Multi-Layered Authentication & Authorization
* **JWT-based Authentication** with secure token management.
* **OTP Verification**: Secure email verification using dynamic OTP generation.
* **Password Management**: Password resets flow securely handled via OTP.
* **Granular RBAC (Role-Based Access Control)**: Custom authorization policies distinguishing between `Admin`, `HR`, `Manager`, `ProjectManager`, `TeamLeader`, and `Employee`.

### 👤 Comprehensive Profile & Settings
* **Profile Management**: Update personal info, role designations (Seniority/Department), and skills.
* **Profile Images**: Upload and manage profile pictures securely stored on the server.
* **User Preferences**: End-to-end management of settings including UI Theme (Light/Dark), Language (Localization), and Timezone.
* **Notification Preferences**: Toggle in-app (SignalR) and email notifications based on individual user preference.

### 👥 User & Workspace Management
* **HR / Admin Controls**: Complete user lifecycle management, role assignments, profile approvals, and account lockouts (Ban/Suspend).
* **Workspace Setup**: Multi-workspace support allowing companies to segment whole divisions. Includes specific industry categorizations for deeper analytics.
* **Workspace Linking**: Users are securely linked to specific workspaces, ensuring strict tenant isolation and contextual data queries.
* **Company & Workspace Invitations**: Email-backed invitations to seamlessly onboard new talent directly into their assigned workspace or project, completely integrated with RBAC roles.

### 📊 Advanced Project & Task Management
* **Agile Tooling**: Complete hierarchical track from `Workspace` → `Project` → `Milestone` → `Task`.
* **State Workflows**: Task progression cycles (`Pending` → `InProgress` → `Submitted` → `Completed` / `Rejected`).
* **Circular Dependency Detection**: Prevents infinite loops when linking tasks using DFS algorithms.
* **Task Attachments & Comments**: Build discussions straight into tickets and upload relative documentation.
* **Specialized Teams**: Ability to form customized groups (Frontend, Devops, QA) under a dedicated Team Leader.

### ⏱️ Time Tracking & Analytics
* Start and stop timer integration strictly linked to tasks.
* Automatic time sheet compilation for productivity reviews.

### 🔔 Live Sync & Real-Time Notifications
* Real-time pushed events directly from the Backend using **SignalR**.
* Alerts for newly assigned tasks, status changes, returned work, and mentions.
* Governed strictly by the user's personal *Notification Settings*.

---

## 🛡️ Role-Based Access Control (RBAC) Permissions Matrix

| Feature / Module | Admin | HR | Manager | Project Manager | Team Leader | Employee |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **System Settings & Global Views** | ✔️ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **User & Role Management** | ✔️ | ✔️ | 🔘¹ | ❌ | ❌ | ❌ |
| **Workspace Configuration** | ✔️ | ❌ | ✔️ | ❌ | ❌ | ❌ |
| **Create/Delete Projects** | ✔️ | ❌ | ✔️ | ✔️ | ❌ | ❌ |
| **Manage Milestones & Deadlines** | ✔️ | ❌ | ✔️ | ✔️ | ✔️ | ❌ |
| **Assign Teams to Projects** | ✔️ | ❌ | ✔️ | ✔️ | ❌ | ❌ |
| **Create & Assign Tasks** | ✔️ | ❌ | ✔️ | ✔️ | ✔️ | ❌ |
| **Review Submitted Tasks** | ✔️ | ❌ | ✔️ | ✔️ | ✔️ | 🔘² |
| **Execute Tasks & Log Time** | ✔️ | ❌ | ✔️ | ✔️ | ✔️ | ✔️ |
| **Personal Profile & Settings** | ✔️ | ✔️ | ✔️ | ✔️ | ✔️ | ✔️ |

*Notes:*
*   *¹ `Managers` can invite and manage employees specifically within their own Workspace boundaries.*
*   *² Specific `Employees` assigned with `QA` or `Reviewer` roles inside a project can review/verify tasks.*

---

## 📈 Role-Specific Dashboards (Analytics) 

A major highlight of SyncVerse is its comprehensive, role-segregated analytical dashboards giving each tier of management exactly the data they need:

*   **👑 Admin Dashboard**: Top-level system metrics, total workspaces, and global task states.
*   **🏢 Manager / Owner Dashboard**: Workspace growth matrices, overall resource utilization, department headcounts, hierarchy charting, and deep dives into internal teams.
*   **🧑‍💼 HR Dashboard**: Employee turnover tracking, ongoing recruitment metric/invitations lifecycle, and department-specific task clearance rates.
*   **📊 Project Manager Dashboard**: Segregated views into managed portfolios, team member density, task completion velocities across specific active projects.
*   **🎯 Team Leader Dashboard**: Live tracking of *Team Workload* (who is overwhelmed vs. idle), Blockers Radar (overdue/rejected tasks), and Team Velocity calculations.
*   **💼 Employee / Member Dashboard**: personalized views detailing "My Tasks", impending deadlines, unread alerts, recent task discussions, and QA/Reviewer-specific validation grids.

---

## 🏛️ Architecture

This project strictly adheres to **Clean Architecture** patterns, ensuring high testability, extremely loose coupling, and organized dependency flow:

*   **Domain Layer**: Core business models, Entities (`User`, `Project`, `TaskItem`, `UserSettings`), and Enums.
*   **Application Layer**: Interfaces, DTOs, Application Logic, and Results wrappers. Contains core services (e.g., `TaskService`, `DashboardService`, `ProfileService`). No UI/DB dependencies.
*   **Infrastructure Layer**: EF Core `DbContext`, Identity configuration, File Storage configurations, SignalR hubs, and Email Services.
*   **Web API (Presentation) Layer**: Minimal Controllers mapping HTTP logic into the Application layer, Swagger configurations, robust Middleware for exception handling, and JWT Bearer settings.

---

## 💻 Technology Stack

*   **Framework**: .NET 8.0 C#
*   **Database**: Microsoft SQL Server / Entity Framework Core (EF Core 8)
*   **Authentication**: ASP.NET Core Identity & JWT
*   **Real-time Web**: SignalR
*   **Data Mapping**: AutoMapper
*   **File Storage**: Local AppData Storage (Extendable to Azure Blob / AWS S3)
*   **Validation**: FluentValidation
*   **API Documentation**: Swagger / OpenAPI

---

## 🚀 Getting Started

### Prerequisites
*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   SQL Server (LocalDB or Developer Edition)
*   Visual Studio 2022 / VS Code / Rider

### Local Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/syncverse12/Backend.Net.git
   cd Backend.Net
   ```

2. **Configure Database Connection:**
   Navigate to `appsettings.json` in the API project and update the `Default` connection string to your local SQL server. Don't forget to configure the JWT Secret Key and Email SMTP credentials.
   
3. **Apply EF Core Migrations:**
   ```bash
   dotnet tool install --global dotnet-ef  # If not already installed
   dotnet ef database update -p ./Infrastructure/SyncVerse.Infrastructure.csproj -s ./SyncVerse.csproj
   ```

4. **Run the API:**
   ```bash
   dotnet run --project ./SyncVerse.csproj
   ```
   Or simply hit `F5` in Visual Studio.


## 📚 API Documentation

Once the application is running navigate to:
`https://localhost:<port>/swagger/index.html` 
to view the dynamically generated Swagger UI encompassing all endpoints, models, and authorization behaviors.

A Postman collection is also available for testing the API endpoints locally. You can find it in the repository under `postman/SyncVerse.postman_collection.json`. Simply import this file into Postman.

---

