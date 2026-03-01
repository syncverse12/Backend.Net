using SyncVerse.Domain.Common;
using SyncVerse.Domain.Enums;

namespace SyncVerse.Domain.Entities  
{
    public class TaskEmployee : BaseEntity
    {
        public string TaskTitle { get; set; } = null!;
        public string? Description { get; set; }

        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }

        public DateTime? Deadline { get; set; }

        public int ProgressPercentage { get; set; }

        public string? AssignedUserId { get; set; }
        public string? ProjectId { get; set; }

        public User AssignedUser { get; set; } = null!;
        public Project Project { get; set; } = null!;

        public ICollection<TimeLog> TimeLogs { get; set; } = new HashSet<TimeLog>();
    }
}
