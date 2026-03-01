using SyncVerse.Domain.Common;
using SyncVerse.Domain.Enums;

namespace SyncVerse.Domain.Entities
{
    public class ProjectInvitation : BaseEntity
    {
        public string ProjectId { get; set; } = null!;
        public Project Project { get; set; } = null!;
        public string EmployeeId { get; set; }=null!;
        public ProjectRole Role { get; set; } 

        public string SentByManagerId { get; set; } = null!;

        public InvitationType Type { get; set; } 
        public InvitationStatus Status { get; set; } 

        public string? RejectionReason { get; set; }

        public DateTime SentAt { get; set; }
    }

}
