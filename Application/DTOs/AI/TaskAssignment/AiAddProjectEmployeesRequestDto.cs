using System.ComponentModel.DataAnnotations;

namespace SyncVerse.Application.DTOs.AI.TaskAssignment
{
    public class AiAddProjectEmployeesRequestDto
    {
        [Required]
        public string ProjectId { get; set; } = string.Empty;
    }
}
