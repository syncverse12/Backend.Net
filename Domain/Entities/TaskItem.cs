using Graduation_Project.Domain.Common;

namespace Graduation_Project.Domain.Entities
{
    public class TaskItem : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public bool IsCompleted { get; set; } = false;

        public string UserId { get; set; } = null!;
        public DateTime? DueDate { get; set; }
    }
}
