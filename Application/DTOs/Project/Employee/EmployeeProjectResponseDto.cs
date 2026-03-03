namespace SyncVerse.Application.DTOs.Project.Employee
{
    public class EmployeeProjectResponseDto
    {
        public string ProjectId { get; set; } = null!;
        public string ProjectName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalTasks { get; set; }
        public int MyTasks { get; set; }
        public int CompletedTasks { get; set; }
        public DateTime JoinedAt { get; set; }
        public string? RepositoryUrl { get; set; }
        public string? DocumentationUrl { get; set; }
    }
}
