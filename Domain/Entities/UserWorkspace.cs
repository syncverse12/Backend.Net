using SyncVerse.Domain.Common;

namespace SyncVerse.Domain.Entities
{
    public class UserWorkspace : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        public string WorkspaceId { get; set; } = null!;
        public Workspace Workspace { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}