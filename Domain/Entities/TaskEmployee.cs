using Graduation_Project.Domain.Common;
using Graduation_Project.Domain.Enums;
using Graduation_Project.Domain.Entities;


namespace Synverse.Domain.Entities
{
    public class TaskEmployee : BaseEntity
    {
        public string TaskTitle { get; set; } = null!;
        public string? Description { get; set; }

        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }

        public DateTime? Deadline { get; set; }

        public int ProgressPercentage { get; set; }

        // Relations
        public string? AssignedUserId { get; set; }
        public string? ProjectId { get; set; }

        // Navigation Properties
        public User AssignedUser { get; set; } = null!;
        public Project Project { get; set; } = null!;

        public ICollection<TimeLog> TimeLogs { get; set; } = new HashSet<TimeLog>();
    }
}
