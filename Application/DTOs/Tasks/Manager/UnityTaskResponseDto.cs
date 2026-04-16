namespace SyncVerse.Application.DTOs.Tasks.Manager
{
    public class UnityTaskResponseDto
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Status { get; set; } = null!; // Todo, Doing, Done
        public string? AssigneeId { get; set; }
    }
}
