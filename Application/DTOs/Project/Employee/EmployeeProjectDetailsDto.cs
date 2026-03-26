using SyncVerse.Application.DTOs.Dashboard;
using SyncVerse.Application.DTOs.Milestones;

namespace SyncVerse.Application.DTOs.Project.Employee
{
    public class EmployeeProjectDetailsDto
    {
        public string ProjectId { get; set; } = null!;
        public string ProjectName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalTasks { get; set; }
        public int MyTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public DateTime JoinedAt { get; set; }
        public string? RepositoryUrl { get; set; }
        public string? DocumentationUrl { get; set; }
        public List<MilestoneResponseDto> Milestones { get; set; } = new();
        public List<ProjectTeamMemberDto> TeamMembers { get; set; } = new();
    }
}
