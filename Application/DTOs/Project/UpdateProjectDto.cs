namespace Graduation_Project.Application.DTOs.Project
{
    public class UpdateProjectDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? Budget { get; set; }
    }
}
