using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Project;
using Graduation_Project.Application.DTOs.Project.Employee;

namespace Graduation_Project.Application.Services.Project.Employee
{
    public interface IEmployeeProjectService
    {
        Task<Result<bool>> RespondToInvitationAsync(string invitationId, RespondInvitationDto dto, string employeeId);
        Task<Result<List<ProjectInvitationResponseDto>>> GetMyInvitationsAsync(string employeeId);
        Task<Result<List<EmployeeProjectResponseDto>>> GetMyProjectsAsync(string employeeId);
        Task<Result<EmployeeProjectDetailsDto>> GetProjectDetailsAsync(string projectId, string employeeId);
    }

}