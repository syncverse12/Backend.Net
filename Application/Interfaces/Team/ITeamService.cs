using SyncVerse.Application.Common.Results;

namespace SyncVerse.Application.Interfaces
{
    public interface ITeamService
    {
        Task<Result<bool>> InviteMemberAsync(
            InviteTeamMemberDto dto,
            string managerId);

        Task<Result<List<TeamMemberResponseDto>>> GetProjectTeamMembersAsync(
            string projectId,
            string managerId);

        Task<Result<bool>> UpdateMemberRoleAsync(
            UpdateTeamMemberRoleDto dto,
            string managerId);

        Task<Result<bool>> RemoveMemberAsync(
            RemoveTeamMemberDto dto,
            string managerId);



    }
}