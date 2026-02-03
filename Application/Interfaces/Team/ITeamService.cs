using Graduation_Project.Application.Common.Results;

namespace Graduation_Project.Application.Interfaces
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