namespace SyncVerse.Application.DTOs.Project
{
    public class ProjectResponseDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string WorkspaceId { get; set; } = null!;
        public string WorkspaceName { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? Budget { get; set; }
    }
}
