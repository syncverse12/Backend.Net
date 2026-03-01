using SyncVerse.Application.Common.Models;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Tasks;
using SyncVerse.Application.DTOs.Tasks.Employee;
using SyncVerse.Application.DTOs.Tasks.Manager;

namespace SyncVerse.Application.Interfaces.Tasks.Employee
{
    public interface IEmployeeTaskService
    {
        Task<Result<PagedResult<TaskResponseDto>>> GetMyTasksAsync(string userid, TaskQuery query);
        Task<Result<EmployeeTaskDetailsDto>> GetTaskDetailsAsync(string taskId, string userId);
        Task<Result<bool>> StartTaskAsync(string userid, string taskId);
        Task<Result<bool>> SubmitTaskAsync(string taskId, string userId, SubmitTaskDto submitDto);
    }

}
