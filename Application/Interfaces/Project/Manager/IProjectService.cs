using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Project;

namespace Graduation_Project.Application.Interfaces
{
    public interface IProjectService
    {
      Task<Result<ProjectResponseDto>> CreateAsync(CreateProjectDto dto,string managerId);

      Task<Result<ProjectResponseDto>> UpdateAsync(string projectId,UpdateProjectDto dto,string managerId);

      Task<Result<ProjectResponseDto>> GetByIdAsync(string projectId, string managerId);

      Task<Result<List<ProjectResponseDto>>> GetByWorkspaceAsync(string workspaceId, string managerId);

        Task<Result<bool>> DeleteProjectAsync(string projectId, string managerId);
        Task<Result<bool>> RestoreProjectAsync(string projectId, string managerId);

        Task<Result<bool>> InviteEmployeeAsync(string projectId, InviteEmployeeDto dto, string managerId);


    }
}