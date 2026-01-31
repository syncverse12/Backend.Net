using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Workspaces;

namespace Graduation_Project.Application.Interfaces
{
    public interface IWorkspaceService
    {
        Task<Result<WorkspaceResponseDto>> CreateAsync(CreateWorkspaceDto dto, string managerId);
        Task<Result<WorkspaceResponseDto>> UpdateAsync(string workspaceId, UpdateWorkspaceDto dto, string managerId);
        Task<Result<WorkspaceResponseDto>> GetByIdAsync(string workspaceId, string managerId);
        Task<Result<bool>> DeleteAsync(string workspaceId, string managerId);
    }
}