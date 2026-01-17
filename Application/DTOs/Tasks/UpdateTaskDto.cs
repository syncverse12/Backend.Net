using Graduation_Project.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.Application.DTOs.Tasks
{
    public class UpdateTaskDto
    {
        [Required]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsCompleted { get; set; }
        public string? CategoryId { get; set; }
        public TaskPriority Priority { get; set; }
    }
}
