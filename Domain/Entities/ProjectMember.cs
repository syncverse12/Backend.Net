using SyncVerse.Domain.Common;
using SyncVerse.Domain.Enums;

namespace SyncVerse.Domain.Entities
{
    public class ProjectMember : BaseEntity
    {
        public string ProjectId { get; set; } = null!;
        public Project Project { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        public ProjectRole Role { get; set; } 

        // Metadata
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // ✅ Permissions
        public bool CanAssignTasks { get; set; } = false;
        public bool CanReviewTasks { get; set; } = false;
        public bool CanEditProject { get; set; } = false;
    }
}
