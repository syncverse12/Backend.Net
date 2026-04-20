using SyncVerse.Domain.Common;
using SyncVerse.Domain.Enums;

namespace SyncVerse.Domain.Entities
{
    public class Team : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public TeamSpecialization Specialization { get; set; }
        public Department Department { get; set; }

        public string? WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }

        public string CreatedByManagerId { get; set; } = null!;
        public User CreatedByManager { get; set; } = null!;

        public string? TeamLeaderId { get; set; }
        public User? TeamLeader { get; set; }
    }
}
