using SyncVerse.Domain.Common;

namespace SyncVerse.Application.DTOs.Project
{
    public class ProjectInvitationResponseDto
    {
        public string Id { get; set; } = null!;
        public string ProjectId { get; set; } = null!;
        public string ProjectName { get; set; } = null!;
        public string? ProjectDescription { get; set; }
        public string EmployeeId { get; set; } = null!;

        public string SentByManagerId { get; set; } = null!;

        public InvitationType Type { get; set; }
        public InvitationStatus Status { get; set; }

        public string? RejectionReason { get; set; }

        public DateTime SentAt { get; set; }
    }

}
