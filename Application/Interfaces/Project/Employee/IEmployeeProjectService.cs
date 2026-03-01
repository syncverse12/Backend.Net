using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Project;
using SyncVerse.Application.DTOs.Project.Employee;

namespace SyncVerse.Application.Services.Project.Employee
{
    public interface IEmployeeProjectService
    {
        Task<Result<bool>> RespondToInvitationAsync(string invitationId, RespondInvitationDto dto, string employeeId);
        Task<Result<List<ProjectInvitationResponseDto>>> GetMyInvitationsAsync(string employeeId);
        Task<Result<List<EmployeeProjectResponseDto>>> GetMyProjectsAsync(string employeeId);
        Task<Result<EmployeeProjectDetailsDto>> GetProjectDetailsAsync(string projectId, string employeeId);
    }

}