namespace SyncVerse.Application.DTOs.Project.Manager
{
    public class CreateProjectDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string WorkspaceId { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? Budget { get; set; }
        public string? RepositoryUrl { get; set; }
        public string? DocumentationUrl { get; set; }
    }
}
