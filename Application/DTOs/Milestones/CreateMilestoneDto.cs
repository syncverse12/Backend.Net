namespace SyncVerse.Application.DTOs.Milestones
{
    public class CreateMilestoneDto
    {
        public string ProjectId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}