using Graduation_Project.Domain.Enums;

namespace Graduation_Project.Application.DTOs.Tasks
{
    public class TaskFilterDto
    {
        public string? ProjectId { get; set; }
        public string? MilestoneId { get; set; }
        public TaskStatus? Status { get; set; }
        public string? AssignedUserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
