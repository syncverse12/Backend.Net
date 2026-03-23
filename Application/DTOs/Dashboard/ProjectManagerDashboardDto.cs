using System;
using System.Collections.Generic;

namespace SyncVerse.Application.DTOs.Dashboard
{
    public class ProjectManagerDashboardDto
    {
        public int TotalManagedProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }

        public int TotalTeamMembers { get; set; }

        public int TotalProjectTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int SubmittedTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int RejectedTasks { get; set; }
        public int OverdueTasks { get; set; }

        public List<ProjectTeamDto> ProjectTeams { get; set; } = new List<ProjectTeamDto>();
    }

    public class ProjectTeamDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public List<ProjectTeamMemberDto> TeamMembers { get; set; } = new List<ProjectTeamMemberDto>();
    }

    public class ProjectTeamMemberDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
