namespace SyncVerse.Application.DTOs.Milestones
{
    public class MilestoneResponseDto
    {
        public string Id { get; set; } = null!;
        public string ProjectId { get; set; } = null!;
        public string ProjectName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}
