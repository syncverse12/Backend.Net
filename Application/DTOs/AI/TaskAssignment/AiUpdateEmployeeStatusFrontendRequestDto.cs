using System.ComponentModel.DataAnnotations;

namespace SyncVerse.Application.DTOs.AI.TaskAssignment
{
    public class AiUpdateEmployeeStatusFrontendRequestDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        public int ActiveTasks { get; set; }
    }
}
