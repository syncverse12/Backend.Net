using Graduation_Project.Application.Common.Models;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Tasks.Manager;

namespace Graduation_Project.Application.Interfaces.Task.Employee
{
    public interface IEmployeeTaskService
    {
        Task<Result<PagedResult<TaskResponseDto>>> GetMyTasksAsync(TaskQuery query);
        Task<Result<bool>> StartTaskAsync(string taskId);
        Task<Result<bool>> SubmitTaskAsync(string taskId);
    }

}
