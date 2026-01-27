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

        public DateTime? Deadline { get; set; }
        public int ProgressPercentage { get; set; }

        public string ProjectName { get; set; } = null!;
    }

}
