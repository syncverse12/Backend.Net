using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.Application.DTOs.Tasks
{
    public class CreateTaskDto
    {
        [Required]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }
    }
}
