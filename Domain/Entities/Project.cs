using SyncVerse.Domain.Common;
using SyncVerse.Domain.Enums;

namespace SyncVerse.Domain.Entities
{
    public class Project : BaseEntity
    {
            public string Name { get; set; } = null!;
            public string Description { get; set; } = null!;

            public ProjectStatus Status { get; set; } = ProjectStatus.Active;

            public string WorkspaceId { get; set; } = null!;
            public Workspace Workspace { get; set; } = null!;

            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }

            public decimal? Budget { get; set; }

            public string? RepositoryUrl { get; set; }
            public string? DocumentationUrl { get; set; }

            public string CreatedByUserId { get; set; } = string.Empty;
            public User CreatedByUser { get; set; } = null!;
           public virtual ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
           public virtual ICollection<TaskItem> Taskitem { get; set; } = new List<TaskItem>();
           public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();

          public ICollection<TaskEmployee> Tasks { get; set; } = new List<TaskEmployee>();
        public string? TeamId { get; set; }
    }
}
