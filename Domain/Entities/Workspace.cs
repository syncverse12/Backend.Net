using SyncVerse.Domain.Common;
using SyncVerse.Domain.Entities;

namespace SyncVerse.Domain.Entities
{
    public class Workspace : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Industry { get; set; }
        public string Description { get; set; } = null!;
        public string CreatedByUserId { get; set; } = null!;
        public User CreatedByUser { get; set; } = null!;
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
