using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Tasks;

namespace Graduation_Project.Application.Interfaces
{
    public interface ITaskService
    {
        Task<Result<TaskResponseDto>> CreateAsync(CreateTaskDto dto, string userId);
        Task<Result<List<TaskResponseDto>>> GetMyTasksAsync(string userId);
        Task<Result<TaskResponseDto>> UpdateAsync(
            int taskId,
            UpdateTaskDto dto,
            string userId
        );

        Task<Result<bool>> DeleteAsync(int taskId, string userId);
        Task<Result<bool>> RestoreAsync(int taskId, string userId);



    }
}
