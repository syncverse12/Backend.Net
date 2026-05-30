namespace SyncVerse.Application.DTOs.CompanyInvitation
{
    public class SendCompanyInvitationResponseDto
    {
        public string InvitationToken { get; set; } = null!;
        public string InvitationLink { get; set; } = null!;
        public string TeamId { get; set; } = null!;
        public string? WorkspaceId { get; set; }
        public string? OrgCode { get; set; }
        public string Email { get; set; } = null!;
    }
}
