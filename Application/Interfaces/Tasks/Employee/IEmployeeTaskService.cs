using Graduation_Project.Application.Common.Models;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Tasks;
using Graduation_Project.Application.DTOs.Tasks.Employee;
using Graduation_Project.Application.DTOs.Tasks.Manager;

namespace Graduation_Project.Application.Interfaces.Tasks.Employee
{
    public interface IEmployeeTaskService
    {
        Task<Result<PagedResult<TaskResponseDto>>> GetMyTasksAsync(string userid, TaskQuery query);
        Task<Result<EmployeeTaskDetailsDto>> GetTaskDetailsAsync(string taskId, string userId);
        Task<Result<bool>> StartTaskAsync(string userid, string taskId);
        Task<Result<bool>> SubmitTaskAsync(string userid, string taskId);
    }

}
