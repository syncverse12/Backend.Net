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

            var dashboard = new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                TotalAdmins = admins.Count,
                TotalManagers = managers.Count,
                TotalEmployees = employees.Count,

                TotalWorkspaces = await _unitOfWork.Repository<Workspace>().Query().CountAsync(),
                TotalProjects = await projectsQuery.CountAsync(),
                ActiveProjects = await projectsQuery.CountAsync(p => p.EndDate >= now),
                CompletedProjects = await projectsQuery.CountAsync(p => p.EndDate < now),

                TotalTasks = await tasksQuery.CountAsync(),
                PendingTasks = await tasksQuery.CountAsync(t => t.Status == TaskStatus.Pending),
                InProgressTasks = await tasksQuery.CountAsync(t => t.Status == TaskStatus.InProgress),
                SubmittedTasks = await tasksQuery.CountAsync(t => t.Status == TaskStatus.Submitted),
                CompletedTasks = await tasksQuery.CountAsync(t => t.Status == TaskStatus.Completed),
                RejectedTasks = await tasksQuery.CountAsync(t => t.Status == TaskStatus.Rejected),
                OverdueTasks = await tasksQuery.CountAsync(t => t.DueDate.HasValue && t.DueDate < now && t.Status != TaskStatus.Completed)
            };

            return Result<AdminDashboardDto>.Success(dashboard);
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

            var dashboard = new EmployeeDashboardDto
            {
                MyProjectsCount = await _unitOfWork.Repository<ProjectMember>()
                    .Query()
                    .CountAsync(pm => pm.UserId == userId && pm.IsActive),

                MyTasksTotal = await myTasksQuery.CountAsync(),
                PendingTasks = await myTasksQuery.CountAsync(t => t.Status == TaskStatus.Pending),
                InProgressTasks = await myTasksQuery.CountAsync(t => t.Status == TaskStatus.InProgress),
                SubmittedTasks = await myTasksQuery.CountAsync(t => t.Status == TaskStatus.Submitted),
                CompletedTasks = await myTasksQuery.CountAsync(t => t.Status == TaskStatus.Completed),
                RejectedTasks = await myTasksQuery.CountAsync(t => t.Status == TaskStatus.Rejected),
                OverdueTasks = await myTasksQuery.CountAsync(t => t.DueDate.HasValue && t.DueDate < now && t.Status != TaskStatus.Completed),

                UnreadNotifications = await _unitOfWork.Repository<Notification>()
                    .Query()
                    .CountAsync(n => n.UserId == userId && !n.IsRead),

                UploadedFilesCount = await _unitOfWork.Repository<TaskAttachment>()
                    .Query()
                    .CountAsync(a => a.UploadedByUserId == userId)
                    + await _unitOfWork.Repository<ProjectAttachment>()
                    .Query()
                    .CountAsync(a => a.UploadedByUserId == userId),

                NextDueDate = nextDueDate
            };

            return Result<EmployeeDashboardDto>.Success(dashboard);
        }
    }
}
