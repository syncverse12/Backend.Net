using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.Application.DTOs.Tasks
{
    public class CreateTaskDto
    {
        public int? CategoryId { get; set; }
        [Required]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }
    }
}
