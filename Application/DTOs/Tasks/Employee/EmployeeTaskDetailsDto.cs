using Graduation_Project.Domain.Enums;

namespace Graduation_Project.Application.DTOs.Tasks.Employee
{
    public class EmployeeTaskDetailsDto
    {
        public string TaskId { get; set; } = null!;
        public string TaskTitle { get; set; } = null!;
        public string? Description { get; set; }

        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }

        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? ProjectName { get; set; }
        public string? ProjectId { get; set; }

        public string? MilestoneName { get; set; }
        public string? MilestoneId { get; set; }

        public string? CategoryName { get; set; }
        public string? CategoryId { get; set; }

        public string? ReviewComment { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public string CreatedByName { get; set; } = null!;
        public string AssignedToName { get; set; } = null!;
    }

}
