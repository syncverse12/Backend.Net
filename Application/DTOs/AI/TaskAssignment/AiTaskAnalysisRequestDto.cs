using System.ComponentModel.DataAnnotations;

namespace SyncVerse.Application.DTOs.AI.TaskAssignment
{
    public class AiTaskAnalysisRequestDto
    {
        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Requester { get; set; } = "System";

        [Required]
        public string Priority { get; set; } = "Normal";
    }
}
