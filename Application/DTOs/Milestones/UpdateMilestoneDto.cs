namespace Graduation_Project.Application.DTOs.Milestones
{
    public class UpdateMilestoneDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}
