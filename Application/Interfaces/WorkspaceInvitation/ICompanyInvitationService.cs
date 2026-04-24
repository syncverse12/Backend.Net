using System.Threading.Tasks;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.CompanyInvitation;
using SyncVerse.Application.DTOs.Auth;


namespace SyncVerse.Application.Interfaces.WorkspaceInvitation
{
    public interface ICompanyInvitationService
    {
        Task<Result<bool>> SendInvitationAsync(SendCompanyInvitationDto dto, string hrId);
        Task<Result<InvitationDetailsDto>> GetInvitationDetailsAsync(string token);
        Task<Result<AuthResponseDto>> CompleteProfileAsync(CompleteProfileDto dto, string userId);
    }
}
