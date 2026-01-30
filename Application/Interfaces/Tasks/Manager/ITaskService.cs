using Graduation_Project.Application.Common.Pagination;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Tasks.Manager;

namespace Graduation_Project.Application.Interfaces.Task.Manager
{
    public interface ITaskService
    {
        Task<Result<TaskResponseDto>> CreateAsync(CreateTaskDto dto, string userId);
        Task<Result<PagedResult<TaskResponseDto>>> GetManagerTasksAsync(string managerId, TaskQuery query);
        Task<Result<TaskResponseDto>> UpdateAsync(string taskId, UpdateTaskDto dto, string managerId);
        Task<Result<bool>> DeleteAsync(string taskId, string userId);
        Task<Result<bool>> RestoreAsync(string taskId, string userId);
        Task<Result<bool>> AddDependencyAsync(AddTaskDependencyDto dto, string userId);
        Task<Result<bool>> ConfirmTaskAsync(string taskId, string managerId);
        Task<Result<bool>> RejectTaskAsync(string taskId, string managerId, string comment);
        Task<Result<ManagerTaskDashboardDto>> GetManagerDashboardAsync(string managerId);
        
    }
}
