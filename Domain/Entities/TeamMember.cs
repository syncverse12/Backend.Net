using SyncVerse.Domain.Common;
using SyncVerse.Domain.Enums;

namespace SyncVerse.Domain.Entities
{
    public class TeamMember : BaseEntity
    {
        public string TeamId { get; set; } = null!;
        public Team Team { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;
        public ProjectRole Role { get; set; }

        public bool IsActive { get; set; } = false; 
    }
}
