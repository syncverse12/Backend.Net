using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.DTOs.WorkspaceInvitation
{
    public class SendCompanyInvitationDto
    {
        public string Email { get; set; } = null!;
        public string TeamId { get; set; } = null!;
        public SeniorityLevel SeniorityLevel { get; set; }
        public ProjectRole Role { get; set; } 
    }
}