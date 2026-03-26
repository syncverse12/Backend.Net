using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Dashboard;
using SyncVerse.Application.Interfaces.Dashboard;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Domain.Entities;
using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public DashboardService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<Result<ManagerDashboardDto>> GetManagerDashboardAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<ManagerDashboardDto>.Failure("Unauthorized");

            var manager = await _userManager.FindByIdAsync(userId);
            if (manager == null || string.IsNullOrEmpty(manager.WorkspaceId))
                return Result<ManagerDashboardDto>.Failure("Manager or Workspace not found");

            var workspaceId = manager.WorkspaceId;
            var now = DateTime.UtcNow;
            
            var totalEmployees = await _userManager.Users.CountAsync(u => u.WorkspaceId == workspaceId);
            var totalProjects = await _unitOfWork.Repository<SyncVerse.Domain.Entities.Project>()
                .Query()
                .CountAsync(p => p.WorkspaceId == workspaceId);

            // Workspace Growth (Simple metric: Users joined this month vs last month)
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            var employeesCreatedThisMonth = await _userManager.Users
                .CountAsync(u => u.WorkspaceId == workspaceId && u.CreatedAt >= thisMonthStart);
            
            var employeesCreatedLastMonth = await _userManager.Users
                .CountAsync(u => u.WorkspaceId == workspaceId && u.CreatedAt >= lastMonthStart && u.CreatedAt < thisMonthStart);

            double growth = 0;
            if (employeesCreatedLastMonth > 0)
            {
                growth = ((double)(employeesCreatedThisMonth - employeesCreatedLastMonth) / employeesCreatedLastMonth) * 100;
            }
            else if (employeesCreatedThisMonth > 0)
            {
                growth = 100;
            }

            // Resource Utilization (Users assigned to at least one active project / Total Users)
            var usersInProjects = await _unitOfWork.Repository<ProjectMember>()
                .Query()
                .Include(pm => pm.Project)
                .Where(pm => pm.IsActive && pm.Project!.WorkspaceId == workspaceId)
                .Select(pm => pm.UserId)
                .Distinct()
                .CountAsync();

            double resourceUtilization = 0;
            if (totalEmployees > 0)
            {
                resourceUtilization = ((double)usersInProjects / totalEmployees) * 100;
            }

            // Department Breakdown
            var departmentBreakdown = await _userManager.Users
                .Where(u => u.WorkspaceId == workspaceId)
                .GroupBy(u => u.Department)
                .Select(g => new DepartmentOverviewDto
                {
                    DepartmentName = g.Key.ToString() ?? "Unknown",
                    EmployeeCount = g.Count()
                })
                .ToListAsync();

            // Managed Teams (Assuming created by the manager for now, ideally by workspaceId)
            var managedTeams = await _unitOfWork.Repository<SyncVerse.Domain.Entities.Team>()
                .Query()
                .Where(t => t.CreatedByManagerId == userId || t.CreatedByManager.WorkspaceId == workspaceId)
                .Include(t => t.TeamLeader)
                .Select(t => new ManagedTeamDto
                {
                    TeamId = t.Id,
                    TeamName = t.Name,
                    TeamLeaderName = t.TeamLeader != null ? t.TeamLeader.FirstName + " " + t.TeamLeader.LastName : "Unassigned"
                })
                .ToListAsync();

            // Hierarchy (Role counts)
            var hierarchy = new List<HierarchyNodeDto>();
            
            var allUsers = await _userManager.Users.Where(u => u.WorkspaceId == workspaceId).ToListAsync();
            int adminsCount = 0, managersCount = 0, employeesCount = 0;
            
            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin")) adminsCount++;
                else if (roles.Contains("Manager") || roles.Contains("WorkspaceAdmin")) managersCount++;
                else employeesCount++;
            }

            hierarchy.Add(new HierarchyNodeDto { RoleName = "Admins (Owners)" ?? string.Empty, NumberOfEmployees = adminsCount });
            hierarchy.Add(new HierarchyNodeDto { RoleName = "Managers" ?? string.Empty, NumberOfEmployees = managersCount });
            hierarchy.Add(new HierarchyNodeDto { RoleName = "Employees" ?? string.Empty, NumberOfEmployees = employeesCount });

            // 4. Quick Actions
            var quickActions = new List<QuickActionDto>
            {
                new QuickActionDto
                {
                    ActionName = "Add New Manager",
                    Description = "Appoint a new manager to the workspace.",
                    ActionType = "AddNewManager"
                },
                new QuickActionDto
                {
                    ActionName = "Create Department",
                    Description = "Create a new department in the company.",
                    ActionType = "CreateDepartment"
                },
                new QuickActionDto
                {
                    ActionName = "Invite Member",
                    Description = "Invite a new employee to join the company.",
                    ActionType = "InviteMember"
                }
            };

            // 5. Project Teams
            var projectTeamsInfo = await _unitOfWork.Repository<ProjectMember>()
                .Query()
                .Include(pm => pm.Project)
                .Include(pm => pm.User)
                .Where(pm => pm.IsActive && pm.Project!.WorkspaceId == workspaceId)
                .GroupBy(pm => new { pm.ProjectId, pm.Project!.Name })
                .Select(g => new ProjectTeamDto
                {
                    ProjectId = g.Key.ProjectId,
                    ProjectName = g.Key.Name,
                    TeamMembers = g.Select(m => new ProjectTeamMemberDto
                    {
                        UserId = m.UserId,
                        Name = m.User != null ? m.User.FirstName + " " + m.User.LastName : string.Empty,
                        Role = m.Role.ToString()
                    }).ToList()
                })
                .ToListAsync();

            var dashboard = new ManagerDashboardDto
            {
                TotalEmployees = totalEmployees,
                TotalProjects = totalProjects,
                WorkspaceGrowthPercentage = Math.Round(growth, 2),
                ResourceUtilizationPercentage = Math.Round(resourceUtilization, 2),
                DepartmentBreakdown = departmentBreakdown,
                ManagedTeams = managedTeams,
                Hierarchy = hierarchy,
                QuickActions = quickActions,
                ProjectTeams = projectTeamsInfo
            };

            return Result<ManagerDashboardDto>.Success(dashboard);
        }

        public async Task<Result<AdminDashboardDto>> GetAdminDashboardAsync()
        {
            var now = DateTime.UtcNow;

            var usersQuery = _userManager.Users;
            var totalUsers = await usersQuery.CountAsync();

            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var managers = await _userManager.GetUsersInRoleAsync("Manager");
            var employees = await _userManager.GetUsersInRoleAsync("Employee");

            var projectsQuery = _unitOfWork.Repository<SyncVerse.Domain.Entities.Project>().Query();
            var tasksQuery = _unitOfWork.Repository<TaskItem>().Query();

            var projectStatsList = await projectsQuery
                .GroupBy(p => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Active = g.Count(p => p.Status == ProjectStatus.Active && p.EndDate >= now),
                    Completed = g.Count(p => p.Status == ProjectStatus.Completed || p.EndDate < now)
                })
                .ToListAsync();
            var projectStats = projectStatsList.FirstOrDefault() ?? new { Total = 0, Active = 0, Completed = 0 };

            var taskStatsList = await tasksQuery
                .GroupBy(t => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Pending = g.Count(t => t.Status == TaskStatus.Pending),
                    InProgress = g.Count(t => t.Status == TaskStatus.InProgress),
                    Submitted = g.Count(t => t.Status == TaskStatus.Submitted),
                    Completed = g.Count(t => t.Status == TaskStatus.Completed),
                    Rejected = g.Count(t => t.Status == TaskStatus.Rejected),
                    Overdue = g.Count(t => t.DueDate.HasValue && t.DueDate < now && t.Status != TaskStatus.Completed)
                })
                .ToListAsync();
            var taskStats = taskStatsList.FirstOrDefault() ?? new { Total = 0, Pending = 0, InProgress = 0, Submitted = 0, Completed = 0, Rejected = 0, Overdue = 0 };

            var dashboard = new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                TotalAdmins = admins.Count,
                TotalManagers = managers.Count,
                TotalEmployees = employees.Count,

                TotalWorkspaces = await _unitOfWork.Repository<Workspace>().Query().CountAsync(),
                TotalProjects = projectStats.Total,
                ActiveProjects = projectStats.Active,
                CompletedProjects = projectStats.Completed,

                TotalTasks = taskStats.Total,
                PendingTasks = taskStats.Pending,
                InProgressTasks = taskStats.InProgress,
                SubmittedTasks = taskStats.Submitted,
                CompletedTasks = taskStats.Completed,
                RejectedTasks = taskStats.Rejected,
                OverdueTasks = taskStats.Overdue
            };

            return Result<AdminDashboardDto>.Success(dashboard);
        }

        public async Task<Result<ProjectManagerDashboardDto>> GetProjectManagerDashboardAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<ProjectManagerDashboardDto>.Failure("Unauthorized");

            var now = DateTime.UtcNow;

            var myManagedProjectIdsQuery = _unitOfWork.Repository<ProjectMember>()
                .Query()
                .Where(pm => pm.UserId == userId && pm.Role == ProjectRole.ProjectManager && pm.IsActive)
                .Select(pm => pm.ProjectId);

            var myManagedProjectIds = await myManagedProjectIdsQuery.ToListAsync();

            if (!myManagedProjectIds.Any())
            {
                return Result<ProjectManagerDashboardDto>.Success(new ProjectManagerDashboardDto());
            }

            var projectsQuery = _unitOfWork.Repository<SyncVerse.Domain.Entities.Project>()
                .Query()
                .Where(p => myManagedProjectIds.Contains(p.Id));

            var teamMembersQuery = _unitOfWork.Repository<ProjectMember>()
                .Query()
                .Where(pm => myManagedProjectIds.Contains(pm.ProjectId) && pm.IsActive);

            var tasksQuery = _unitOfWork.Repository<TaskItem>()
                .Query()
                .Where(t => t.ProjectId != null && myManagedProjectIds.Contains(t.ProjectId));

            var totalTeamMembers = await teamMembersQuery.Select(pm => pm.UserId).Distinct().CountAsync();

            var projectStatsList = await projectsQuery
                .GroupBy(p => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Active = g.Count(p => p.Status == ProjectStatus.Active && p.EndDate >= now),
                    Completed = g.Count(p => p.Status == ProjectStatus.Completed || p.EndDate < now)
                })
                .ToListAsync();
            var projectStats = projectStatsList.FirstOrDefault() ?? new { Total = 0, Active = 0, Completed = 0 };

            var taskStatsList = await tasksQuery
                .GroupBy(t => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Pending = g.Count(t => t.Status == TaskStatus.Pending),
                    InProgress = g.Count(t => t.Status == TaskStatus.InProgress),
                    Submitted = g.Count(t => t.Status == TaskStatus.Submitted),
                    Completed = g.Count(t => t.Status == TaskStatus.Completed),
                    Rejected = g.Count(t => t.Status == TaskStatus.Rejected),
                    Overdue = g.Count(t => t.DueDate.HasValue && t.DueDate < now && t.Status != TaskStatus.Completed)
                })
                .ToListAsync();
            var taskStats = taskStatsList.FirstOrDefault() ?? new { Total = 0, Pending = 0, InProgress = 0, Submitted = 0, Completed = 0, Rejected = 0, Overdue = 0 };

            var projectTeamsInfo = await _unitOfWork.Repository<ProjectMember>()
                .Query()
                .Include(pm => pm.Project)
                .Include(pm => pm.User)
                .Where(pm => myManagedProjectIds.Contains(pm.ProjectId) && pm.IsActive)
                .GroupBy(pm => new { pm.ProjectId, pm.Project!.Name })
                .Select(g => new ProjectTeamDto
                {
                    ProjectId = g.Key.ProjectId,
                    ProjectName = g.Key.Name,
                    TeamMembers = g.Select(m => new ProjectTeamMemberDto
                    {
                        UserId = m.UserId,
                        Name = m.User != null ? m.User.FirstName + " " + m.User.LastName : string.Empty,
                        Role = m.Role.ToString()
                    }).ToList()
                })
                .ToListAsync();

            var dashboard = new ProjectManagerDashboardDto
            {
                TotalManagedProjects = projectStats.Total,
                ActiveProjects = projectStats.Active,
                CompletedProjects = projectStats.Completed,

                TotalTeamMembers = totalTeamMembers,

                TotalProjectTasks = taskStats.Total,
                PendingTasks = taskStats.Pending,
                InProgressTasks = taskStats.InProgress,
                SubmittedTasks = taskStats.Submitted,
                CompletedTasks = taskStats.Completed,
                RejectedTasks = taskStats.Rejected,
                OverdueTasks = taskStats.Overdue,

                ProjectTeams = projectTeamsInfo
            };

            return Result<ProjectManagerDashboardDto>.Success(dashboard);
        }

        public async Task<Result<TeamLeaderDashboardDto>> GetTeamLeaderDashboardAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<TeamLeaderDashboardDto>.Failure("Unauthorized");

            var now = DateTime.UtcNow;
            
            // Get projects where user is TeamLeader
            var myLeadProjectIds = await _unitOfWork.Repository<ProjectMember>()
                .Query()
                .Where(pm => pm.UserId == userId && pm.Role == ProjectRole.TeamLeader && pm.IsActive)
                .Select(pm => pm.ProjectId)
                .ToListAsync();

            if (!myLeadProjectIds.Any())
                return Result<TeamLeaderDashboardDto>.Success(new TeamLeaderDashboardDto());

            var tasksQuery = _unitOfWork.Repository<TaskItem>()
                .Query()
                .Where(t => myLeadProjectIds.Contains(t.ProjectId ?? string.Empty));

            // 1. Team Workload
            var teamWorkloadData = await tasksQuery
                .Where(t => t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress)
                .GroupBy(t => t.AssignedToUserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    FirstOrDefault = g.FirstOrDefault(),
                    PendingTasks = g.Count(x => x.Status == TaskStatus.Pending),
                    InProgressTasks = g.Count(x => x.Status == TaskStatus.InProgress)
                })
                .ToListAsync();

            var teamWorkload = teamWorkloadData
                .Select(g => new TeamWorkloadDto
                {
                    UserId = g.UserId,
                    UserName = g.FirstOrDefault != null && g.FirstOrDefault.AssignedToUser != null ? g.FirstOrDefault.AssignedToUser.FirstName + " " + g.FirstOrDefault.AssignedToUser.LastName : string.Empty,
                    PendingTasks = g.PendingTasks,
                    InProgressTasks = g.InProgressTasks
                })
                .ToList();

            // 2. Pending Reviews
            var pendingReviews = await tasksQuery
                .Where(t => t.Status == TaskStatus.Submitted)
                .OrderBy(t => t.SubmittedAt)
                .Select(t => new PendingReviewDto
                {
                    TaskId = t.Id,
                    TaskTitle = t.Title,
                    SubmittedByUserId = t.AssignedToUserId,
                    SubmittedByUserName = t.AssignedToUser != null ? t.AssignedToUser.FirstName + " " + t.AssignedToUser.LastName : string.Empty,
                    SubmittedAt = t.SubmittedAt,
                    ProjectId = t.ProjectId ?? string.Empty,
                    ProjectName = t.Project != null ? t.Project.Name : string.Empty,
                })
                .ToListAsync();

            // 3. Blockers Radar (Overdue or Rejected tasks)
            var blockers = await tasksQuery
                .Where(t => t.Status == TaskStatus.Rejected || (t.DueDate.HasValue && t.DueDate < now && t.Status != TaskStatus.Completed))
                .Select(t => new BlockerDto
                {
                    TaskId = t.Id,
                    TaskTitle = t.Title,
                    ProblemDescription = t.Status == TaskStatus.Rejected ? "Task Rejected" : "Task Overdue",
                    AssignedToUserId = t.AssignedToUserId,
                    AssignedToUserName = t.AssignedToUser != null ? t.AssignedToUser.FirstName + " " + t.AssignedToUser.LastName : string.Empty,
                    ProjectId = t.ProjectId ?? string.Empty,
                    ProjectName = t.Project != null ? t.Project.Name : string.Empty,
                })
                .ToListAsync();

            // 4. Team Velocity
            var thisWeekStart = now.Date.AddDays(-(int)now.DayOfWeek);
            var lastWeekStart = thisWeekStart.AddDays(-7);

            var completedThisWeek = await tasksQuery
                .CountAsync(t => t.Status == TaskStatus.Completed && t.TaskCompletedAt >= thisWeekStart);

            var completedLastWeek = await tasksQuery
                .CountAsync(t => t.Status == TaskStatus.Completed && t.TaskCompletedAt >= lastWeekStart && t.TaskCompletedAt < thisWeekStart);

            double growth = 0;
            if (completedLastWeek > 0)
            {
                growth = ((double)(completedThisWeek - completedLastWeek) / completedLastWeek) * 100;
            }
            else if (completedThisWeek > 0)
            {
                growth = 100; // 100% growth if they accomplished something vs nothing last week.
            }

            var velocity = new TeamVelocityDto
            {
                CompletedThisWeek = completedThisWeek,
                CompletedLastWeek = completedLastWeek,
                GrowthPercentage = Math.Round(growth, 2)
            };

            var dashboard = new TeamLeaderDashboardDto
            {
                TeamWorkload = teamWorkload,
                PendingReviews = pendingReviews,
                BlockersRadar = blockers,
                TeamVelocity = velocity
            };

            return Result<TeamLeaderDashboardDto>.Success(dashboard);
        }

        public async Task<Result<HRDashboardDto>> GetHRDashboardAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<HRDashboardDto>.Failure("Unauthorized");

            var hrUser = await _userManager.FindByIdAsync(userId);
            if (hrUser == null || string.IsNullOrEmpty(hrUser.WorkspaceId))
                return Result<HRDashboardDto>.Failure("HR or Workspace not found");

            var workspaceId = hrUser.WorkspaceId;
            var now = DateTime.UtcNow;

            // 1. Employee Overview
            var employeeOverview = await _userManager.Users
                .Where(u => u.WorkspaceId == workspaceId)
                .GroupBy(u => u.Department)
                .Select(g => new DepartmentOverviewDto
                {
                    DepartmentName = g.Key.ToString() ?? "Unknown",
                    EmployeeCount = g.Count()
                })
                .ToListAsync();

            // 2. Invitations Tracking (Assuming invitations have SentByHR which links to Workspace)
            var invitationsQuery = _unitOfWork.Repository<CompanyInvitation>()
                .Query()
                .Include(i => i.SentByHR)
                .Where(i => i.SentByHR.WorkspaceId == workspaceId);
                
            var invitationsStatsList = await invitationsQuery
                .GroupBy(i => 1)
                .Select(g => new
                {
                    TotalSent = g.Count(),
                    Accepted = g.Count(i => i.Status == InvitationStatus.Accepted),
                    Pending = g.Count(i => i.Status == InvitationStatus.Pending),
                    Expired = g.Count(i => i.Status == InvitationStatus.Expired),
                    Cancelled = g.Count(i => i.Status == InvitationStatus.Cancelled)
                })
                .ToListAsync();

            var invitationsStats = invitationsStatsList.FirstOrDefault() ?? new { TotalSent = 0, Accepted = 0, Pending = 0, Expired = 0, Cancelled = 0 };

            var recentInvitations = await invitationsQuery
                .OrderByDescending(i => i.SentAt)
                .Take(5)
                .Select(i => new RecentInvitationDto
                {
                    Email = i.Email,
                    Status = i.Status.ToString(),
                    SentAt = i.SentAt,
                    Role = i.Role.ToString()
                })
                .ToListAsync();

            var trackingDto = new InvitationTrackingDto
            {
                TotalSent = invitationsStats.TotalSent,
                Accepted = invitationsStats.Accepted,
                Pending = invitationsStats.Pending,
                Expired = invitationsStats.Expired,
                Cancelled = invitationsStats.Cancelled,
                RecentInvitations = recentInvitations
            };

            // 3. Department Performance
            var tasksQuery = _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.AssignedToUser)
                .Where(t => t.AssignedToUser != null && t.AssignedToUser.WorkspaceId == workspaceId);

            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

            var departmentTasks = await tasksQuery
                .GroupBy(t => t.AssignedToUser!.Department)
                .Select(g => new DepartmentPerformanceDto
                {
                    DepartmentName = g.Key.ToString(),
                    ActiveTasks = g.Count(t => t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress),
                    CompletedTasksThisMonth = g.Count(t => t.Status == TaskStatus.Completed && t.TaskCompletedAt >= firstDayOfMonth)
                })
                .ToListAsync();

            var dashboard = new HRDashboardDto
            {
                EmployeeOverview = employeeOverview,
                InvitationsTracking = trackingDto,
                DepartmentPerformance = departmentTasks
            };

            return Result<HRDashboardDto>.Success(dashboard);
        }

        public async Task<Result<EmployeeDashboardDto>> GetEmployeeDashboardAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<EmployeeDashboardDto>.Failure("Unauthorized");

            var now = DateTime.UtcNow;

            var myTasksQuery = _unitOfWork.Repository<TaskItem>()
                .Query()
                .Where(t => t.AssignedToUserId == userId);

            var nextDueDate = await myTasksQuery
                .Where(t => t.DueDate.HasValue && t.Status != TaskStatus.Completed)
                .OrderBy(t => t.DueDate)
                .Select(t => t.DueDate)
                .FirstOrDefaultAsync();

            var taskStatsList = await myTasksQuery
                .GroupBy(t => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Pending = g.Count(t => t.Status == TaskStatus.Pending),
                    InProgress = g.Count(t => t.Status == TaskStatus.InProgress),
                    Submitted = g.Count(t => t.Status == TaskStatus.Submitted),
                    Completed = g.Count(t => t.Status == TaskStatus.Completed),
                    Rejected = g.Count(t => t.Status == TaskStatus.Rejected),
                    Overdue = g.Count(t => t.DueDate.HasValue && t.DueDate < now && t.Status != TaskStatus.Completed)
                })
                .ToListAsync();
            var taskStats = taskStatsList.FirstOrDefault() ?? new { Total = 0, Pending = 0, InProgress = 0, Submitted = 0, Completed = 0, Rejected = 0, Overdue = 0 };

            var todayStart = now.Date;
            var todayEnd = now.Date.AddDays(1);

            var todayTasks = await myTasksQuery
                .Where(t => t.DueDate.HasValue && t.DueDate.Value >= todayStart && t.DueDate.Value < todayEnd && t.Status != TaskStatus.Completed)
                .Select(t => new DailyTaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    DueDate = t.DueDate
                })
                .ToListAsync();

            var myTaskIds = myTasksQuery.Select(t => t.Id);
            
            var recentComments = await _unitOfWork.Repository<TaskComment>()
                .Query()
                .Include(c => c.User)
                .Include(c => c.Task)
                .Where(c => myTaskIds.Contains(c.TaskId) && c.UserId != userId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new RecentCommentDto
                {
                    Id = c.Id,
                    TaskId = c.TaskId,
                    TaskTitle = c.Task != null ? c.Task.Title : string.Empty,
                    Content = c.Content,
                    CommentByUserId = c.UserId,
                    CommentByUserName = c.User != null ? c.User.FirstName + " " + c.User.LastName : string.Empty,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            var myProjectIdsQuery = _unitOfWork.Repository<ProjectMember>()
                .Query()
                .Where(pm => pm.UserId == userId && pm.IsActive)
                .Select(pm => pm.ProjectId);

            var upcomingMilestones = await _unitOfWork.Repository<Milestone>()
                .Query()
                .Include(m => m.Project)
                .Where(m => myProjectIdsQuery.Contains(m.ProjectId) && !m.IsCompleted && m.EndDate >= now.Date)
                .OrderBy(m => m.EndDate)
                .Take(5)
                .Select(m => new UpcomingMilestoneDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    ProjectId = m.ProjectId,
                    ProjectName = m.Project != null ? m.Project.Name : string.Empty,
                    EndDate = m.EndDate,
                    IsCompleted = m.IsCompleted
                })
                .ToListAsync();

            // Role-Specific queries (Reviewer / QA)
            var myRolesQuery = _unitOfWork.Repository<ProjectMember>()
                .Query()
                .Where(pm => pm.UserId == userId && pm.IsActive)
                .Select(pm => new { pm.ProjectId, pm.Role });
            
            var userRoles = await myRolesQuery.ToListAsync();

            var reviewerProjectIds = userRoles.Where(r => r.Role == ProjectRole.Reviewer).Select(r => r.ProjectId).ToList();
            var qaProjectIds = userRoles.Where(r => r.Role == ProjectRole.QA).Select(r => r.ProjectId).ToList();

            var tasksPendingMyApproval = new List<DailyTaskDto>();
            if (reviewerProjectIds.Any())
            {
                tasksPendingMyApproval = await _unitOfWork.Repository<TaskItem>()
                    .Query()
                    .Where(t => reviewerProjectIds.Contains(t.ProjectId ?? string.Empty) && t.Status == TaskStatus.Submitted)
                    .Select(t => new DailyTaskDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Status = t.Status.ToString(),
                        Priority = t.Priority.ToString(),
                        DueDate = t.DueDate
                    })
                    .ToListAsync();
            }

            var bugsToVerify = new List<DailyTaskDto>();
            if (qaProjectIds.Any())
            {
                 // Assuming Rejected tasks or a specific Bug category means it needs QA verification. 
                 // For now, looking for tasks that might need QA attention (e.g., recently submitted or rejected for verification).
                 // Adjust this query based on exact workflow for QA.
                bugsToVerify = await _unitOfWork.Repository<TaskItem>()
                    .Query()
                    .Where(t => qaProjectIds.Contains(t.ProjectId ?? string.Empty) && (t.Status == TaskStatus.Submitted || t.Status == TaskStatus.Rejected))
                    .Select(t => new DailyTaskDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Status = t.Status.ToString(),
                        Priority = t.Priority.ToString(),
                        DueDate = t.DueDate
                    })
                    .ToListAsync();
            }

            var dashboard = new EmployeeDashboardDto
            {
                MyProjectsCount = await _unitOfWork.Repository<ProjectMember>()
                    .Query()
                    .CountAsync(pm => pm.UserId == userId && pm.IsActive),

                MyTasksTotal = taskStats.Total,
                PendingTasks = taskStats.Pending,
                InProgressTasks = taskStats.InProgress,
                SubmittedTasks = taskStats.Submitted,
                CompletedTasks = taskStats.Completed,
                RejectedTasks = taskStats.Rejected,
                OverdueTasks = taskStats.Overdue,

                UnreadNotifications = await _unitOfWork.Repository<Notification>()
                    .Query()
                    .CountAsync(n => n.UserId == userId && !n.IsRead),

                UploadedFilesCount = await _unitOfWork.Repository<TaskAttachment>()
                    .Query()
                    .CountAsync(a => a.UploadedByUserId == userId)
                    + await _unitOfWork.Repository<ProjectAttachment>()
                    .Query()
                    .CountAsync(a => a.UploadedByUserId == userId),

                NextDueDate = nextDueDate,

                TodayTasks = todayTasks,
                RecentComments = recentComments,
                UpcomingMilestones = upcomingMilestones,
                
                TasksPendingMyApproval = tasksPendingMyApproval,
                BugsToVerify = bugsToVerify
            };

            return Result<EmployeeDashboardDto>.Success(dashboard);
        }

        public async Task<Result<ManagerTaskDashboardDto>> GetManagerTaskDashboardAsync(string managerId)
        {
            var manager = await _userManager.FindByIdAsync(managerId);
            if (manager == null || string.IsNullOrEmpty(manager.WorkspaceId))
                return Result<ManagerTaskDashboardDto>.Failure("Manager or Workspace not found");

            var workspaceId = manager.WorkspaceId;

            var managerProjectIds = await _unitOfWork.Repository<SyncVerse.Domain.Entities.Project>().Query()
                .Where(p => p.WorkspaceId == workspaceId)
                .Select(p => p.Id)
                .ToListAsync();


            var tasks = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .IgnoreQueryFilters() 
                .Include(t => t.AssignedToUser)
                .Include(t => t.Category)
                .Where(t => t.ProjectId != null && managerProjectIds.Contains(t.ProjectId!))
                .ToListAsync();

            var statusStats = new TaskStatusStatsDto
            {
                Total = tasks.Count(t => !t.IsDeleted),
                Pending = tasks.Count(t => !t.IsDeleted && t.Status == TaskStatus.Pending),
                InProgress = tasks.Count(t => !t.IsDeleted && t.Status == TaskStatus.InProgress),
                Submitted = tasks.Count(t => !t.IsDeleted && t.Status == TaskStatus.Submitted),
                Completed = tasks.Count(t => !t.IsDeleted && t.Status == TaskStatus.Completed),
                Rejected = tasks.Count(t => !t.IsDeleted && t.Status == TaskStatus.Rejected)
            };

            var tasksPerEmployee = tasks
                .Where(t => !t.IsDeleted && !string.IsNullOrEmpty(t.AssignedToUserId))
                .GroupBy(t => new
                {
                    t.AssignedToUserId,
                    EmployeeName = t.AssignedToUser != null
                        ? $"{t.AssignedToUser.FirstName} {t.AssignedToUser.LastName}".Trim()
                        : "Unknown"
                })
                .Select(g => new EmployeeTaskStatsDto
                {
                    EmployeeId = g.Key.AssignedToUserId!,
                    EmployeeName = g.Key.EmployeeName,
                    TasksCount = g.Count()
                })
                .ToList();

            return Result<ManagerTaskDashboardDto>.Success(new ManagerTaskDashboardDto
            {
                StatusStats = statusStats,
                TasksPerEmployee = tasksPerEmployee,
                TasksPerCategory = tasks
                    .Where(t => t.Category != null && !t.IsDeleted) 
                    .GroupBy(t => t.Category!.Name)
                    .Select(g => new CategoryTaskStatsDto { CategoryName = g.Key, TasksCount = g.Count() })
                    .ToList()
            });
        }

        public async Task<Result<TaskDashboardDto>> GetProjectTaskDashboardAsync(string projectId, string managerId)
        {
            var isAuthorized = await _unitOfWork.Repository<SyncVerse.Domain.Entities.Project>().Query()
                .Include(p => p.Workspace)
                .Include(p => p.TeamMembers)
                .AnyAsync(p => p.Id == projectId &&
                               (p.CreatedByUserId == managerId ||
                                p.Workspace!.CreatedByUserId == managerId ||
                                p.TeamMembers.Any(m => m.UserId == managerId &&
                                                 (m.Role == ProjectRole.ProjectManager || m.Role == ProjectRole.TeamLeader))));

            if (!isAuthorized)
                return Result<TaskDashboardDto>.Failure("Unauthorized: You don't have permission to view this project's dashboard.");

            var tasks = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.Category)
                .Where(t => t.ProjectId == projectId && !t.IsDeleted)
                .ToListAsync();

            var dashboard = new TaskDashboardDto
            {
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.Status == TaskStatus.Completed),
                InProgressTasks = tasks.Count(t => t.Status == TaskStatus.InProgress),
                OverdueTasks = tasks.Count(t =>
                    t.DueDate.HasValue &&
                    t.DueDate.Value < DateTime.UtcNow &&
                    t.Status != TaskStatus.Completed),

                CategoryStats = tasks
                    .Where(t => t.Category != null)
                    .GroupBy(t => t.Category!.Name)
                    .Select(g => new CategoryTaskStatsDto { CategoryName = g.Key, TasksCount = g.Count() })
                    .ToList()
            };

            return Result<TaskDashboardDto>.Success(dashboard);
        }
    }
}
