using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.DTOs.WorkspaceInvitation
{
    public class InvitationDetailsDto
    {
        public string Email { get; set; } = null!;
        public string TeamId { get; set; } = null!;
        public string TeamName { get; set; } = null!;
        public string? TeamDescription { get; set; }
        public Department Department { get; set; }
        public string DepartmentDisplay { get; set; } = null!;
        public SeniorityLevel SeniorityLevel { get; set; }
        public string SeniorityLevelDisplay { get; set; } = null!;
        public ProjectRole Role { get; set; }  
        public string RoleDisplay { get; set; } = null!;
        public string HRName { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsValid { get; set; }
    }
}
