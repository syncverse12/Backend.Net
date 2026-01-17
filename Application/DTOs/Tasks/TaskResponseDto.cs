using Graduation_Project.Domain.Enums;

namespace Graduation_Project.Application.DTOs.Tasks
{
    public class TaskResponseDto
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskPriority Priority { get; set; }
        public string? CategoryName { get; set; }
        public string AssignedToUserName { get; set; } = null!;
        public string CreatedByUserName { get; set; } = null!;
    }
}
