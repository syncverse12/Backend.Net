using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Dashboard;

namespace SyncVerse.Application.Interfaces.Dashboard
{
    public interface IDashboardService
    {
        Task<Result<ManagerDashboardDto>> GetManagerDashboardAsync(string userId);
        Task<Result<AdminDashboardDto>> GetAdminDashboardAsync();
        Task<Result<ProjectManagerDashboardDto>> GetProjectManagerDashboardAsync(string userId);
        Task<Result<TeamLeaderDashboardDto>> GetTeamLeaderDashboardAsync(string userId);
        Task<Result<EmployeeDashboardDto>> GetEmployeeDashboardAsync(string userId);
        Task<Result<HRDashboardDto>> GetHRDashboardAsync(string userId);
        Task<Result<ManagerTaskDashboardDto>> GetManagerTaskDashboardAsync(string managerId);
        Task<Result<TaskDashboardDto>> GetProjectTaskDashboardAsync(string projectId, string managerId);
    }
}
