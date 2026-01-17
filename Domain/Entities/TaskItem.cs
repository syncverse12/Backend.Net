using Graduation_Project.Domain.Common;
using Graduation_Project.Domain.Enums;
using Graduation_Project.Domain.Models;

namespace Graduation_Project.Domain.Entities
{
    public class TaskItem : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public bool IsCompleted { get; set; } = false;

        public string CreatedByUserId { get; set; } = null!;
        public User CreatedByUser { get; set; } = null!;

        public string AssignedToUserId { get; set; } = null!;
        public User AssignedToUser { get; set; } = null!;
        public DateTime? DueDate { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    }
}
