using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Workspaces;

namespace SyncVerse.Application.Interfaces.Workspaces
{
    public interface IWorkspaceSessionService
    {
        Task<Result<WorkspaceSessionDto?>> GetSessionAsync(string orgCode, string userId);
        Task<Result<WorkspaceSessionDto>> CreateSessionAsync(string orgCode, string userId, string joinCode);
        Task<Result<bool>> EndSessionAsync(string orgCode, string userId);
    }
}
