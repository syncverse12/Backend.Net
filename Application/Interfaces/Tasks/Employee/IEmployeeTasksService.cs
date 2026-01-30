using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Tasks.Manager;
using Task = System.Threading.Tasks.Task;
public interface IEmployeeTaskService
{
    Task<Result<bool>> StartTaskAsync(string taskId, string userId);
    Task<Result<bool>> SubmitTaskAsync(string taskId, string userId);
    Task<Result<PagedResult<TaskResponseDto>>> GetMyTasksAsync(string userId, TaskQuery query);
}