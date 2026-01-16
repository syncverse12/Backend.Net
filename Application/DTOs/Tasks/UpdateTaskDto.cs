using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.Application.DTOs.Tasks
{
    public class UpdateTaskDto
    {
        [Required]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsCompleted { get; set; }
    }
}
