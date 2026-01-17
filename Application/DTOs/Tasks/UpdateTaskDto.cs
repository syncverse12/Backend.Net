using Graduation_Project.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.Application.DTOs.Tasks
{
    public class UpdateTaskDto
    {
        [Required]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public TaskStatus Status { get; set; }
        public string? CategoryId { get; set; }
        public TaskPriority Priority { get; set; }
        public string AssignedToUserId { get; set; } = null!;
    }
}
