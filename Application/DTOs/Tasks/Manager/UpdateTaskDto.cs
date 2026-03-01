using SyncVerse.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SyncVerse.Application.DTOs.Tasks.Manager
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
        public DateTime? DueDate { get; set; }
    }
}
