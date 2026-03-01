using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Workspaces;

namespace SyncVerse.Application.Interfaces
{
    public interface IWorkspaceService
    {
        Task<Result<WorkspaceResponseDto>> CreateAsync(CreateWorkspaceDto dto, string managerId);
        Task<Result<WorkspaceResponseDto>> UpdateAsync(string workspaceId, UpdateWorkspaceDto dto, string managerId);
        Task<Result<WorkspaceResponseDto>> GetByIdAsync(string workspaceId, string managerId);
        Task<Result<bool>> DeleteAsync(string workspaceId, string managerId);
        Task<Result<List<WorkspaceResponseDto>>> GetAllAsync(string managerId);
        Task<Result<bool>> RestoreAsync(string workspaceId, string managerId);

    }
}