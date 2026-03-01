using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Project;
using SyncVerse.Application.DTOs.Project.Manager;

namespace SyncVerse.Application.Interfaces
{
    public interface IProjectService
    {
      Task<Result<ProjectResponseDto>> CreateAsync(CreateProjectDto dto,string managerId);

      Task<Result<ProjectResponseDto>> UpdateAsync(string projectId,UpdateProjectDto dto,string managerId);

      Task<Result<ProjectResponseDto>> GetByIdAsync(string projectId, string managerId);

      Task<Result<List<ProjectResponseDto>>> GetByWorkspaceForManagerAsync(string workspaceId, string managerId);

      Task<Result<bool>> DeleteProjectAsync(string projectId, string managerId);
      Task<Result<bool>> RestoreProjectAsync(string projectId, string managerId);

      Task<Result<bool>> InviteEmployeeAsync(string projectId, InviteEmployeeDto dto, string managerId);


    }
}