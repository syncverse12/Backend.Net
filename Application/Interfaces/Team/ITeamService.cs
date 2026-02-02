using Graduation_Project.Application.Common.Results;

namespace Graduation_Project.Application.Interfaces
{
    public interface ITeamService
    {
        System.Threading.Tasks.Task<Result<bool>> InviteMemberAsync(
            InviteTeamMemberDto dto,
            string managerId);
    }
}