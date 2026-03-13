using SyncVerse.Domain.Common;
using SyncVerse.Domain.Enums;

namespace SyncVerse.Domain.Entities
{
    public class CompanyInvitation : BaseEntity
    {
        public string Email { get; set; } = null!;
        
        public string TeamId { get; set; } = null!;
        public Team Team { get; set; } = null!;
        
        public SeniorityLevel SeniorityLevel { get; set; }
        public ProjectRole Role { get; set; }
        
        public string InvitationToken { get; set; } = null!;
        public string SentByHRId { get; set; } = null!;
        public User SentByHR { get; set; } = null!;
        
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
        public DateTime SentAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}