using System.ComponentModel.DataAnnotations;

namespace SyncVerse.Application.DTOs.Tasks.Manager
{
    public class UpdateTaskStatusDto
    {
        [Required]
        public string Status { get; set; } = null!; // Todo, Doing, Done
    }
}
