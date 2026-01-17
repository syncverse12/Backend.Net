using Graduation_Project.Application.Common.Pagination;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Tasks;

namespace Graduation_Project.Application.Interfaces
{
    public interface ITaskService
    {
        Task<Result<TaskResponseDto>> CreateAsync(CreateTaskDto dto, string userId);
        Task<Result<PagedResult<TaskResponseDto>>> GetMyTasksAsync(string userId, TaskQuery query);
        Task<Result<TaskResponseDto>> UpdateAsync(
            string taskId,
            UpdateTaskDto dto,
            string userId
        );

        Task<Result<bool>> DeleteAsync(string taskId, string userId);
        Task<Result<bool>> RestoreAsync(string taskId, string userId);
        Task<Result<bool>> AddDependencyAsync(
            AddTaskDependencyDto dto,
            string userId
        );


    }
}
