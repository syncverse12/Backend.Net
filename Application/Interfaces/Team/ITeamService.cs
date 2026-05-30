using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Team;

namespace SyncVerse.Application.Interfaces.Team
{
    public interface ITeamService
    {
        Task<Result<TeamResponseDto>> CreateTeamAsync(CreateTeamDto dto, string managerId);
        Task<Result<List<TeamResponseDto>>> GetMyTeamsAsync(string userId, string userRole, string workspaceId, string orgCode);
        Task<Result<TeamResponseDto>> GetTeamByIdAsync(string teamId, string userId, string userRole);
        Task<Result<bool>> UpdateTeamAsync(UpdateTeamDto dto, string managerId);
        Task<Result<bool>> DeleteTeamAsync(string teamId, string managerId);
        Task<Result<bool>> RestoreTeamAsync(string teamId, string managerId);
    }
}
