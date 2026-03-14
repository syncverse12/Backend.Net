using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Dashboard;

namespace SyncVerse.Application.Interfaces.Dashboard
{
    public interface IDashboardService
    {
        Task<Result<AdminDashboardDto>> GetAdminDashboardAsync();
        Task<Result<EmployeeDashboardDto>> GetEmployeeDashboardAsync(string userId);
    }
}
