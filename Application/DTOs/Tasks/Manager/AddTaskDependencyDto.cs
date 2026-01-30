using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.Application.DTOs.Tasks.Manager
{
    public class AddTaskDependencyDto
    {
        [Required]
        public string TaskId { get; set; } = null!;

        [Required]
        public string DependsOnTaskId { get; set; } = null!;
    }
}
