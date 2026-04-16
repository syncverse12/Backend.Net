using SyncVerse.Application.Common.Pagination;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Project.Manager;
using SyncVerse.Application.DTOs.Tasks;
using SyncVerse.Application.DTOs.Tasks.Manager;

namespace SyncVerse.Application.Interfaces.Task.Manager
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
        Task<Result<List<TaskResponseDto>>> FilterTasksAsync(TaskFilterDto filter, string managerId);

        // Unity endpoints
        Task<List<UnityTaskResponseDto>> GetUnityTasksAsync(string orgId, string teamId);
        Task<bool> UpdateTaskStatusAsync(string taskId, string status);
    }
}
