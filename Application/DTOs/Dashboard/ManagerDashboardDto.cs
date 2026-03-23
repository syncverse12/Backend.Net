namespace SyncVerse.Application.DTOs.Dashboard
{
    public class ManagerDashboardDto
    {
        public int TotalEmployees { get; set; }
        public int TotalProjects { get; set; }
        public double WorkspaceGrowthPercentage { get; set; }
        public double ResourceUtilizationPercentage { get; set; }
        
        public List<DepartmentOverviewDto> DepartmentBreakdown { get; set; } = new List<DepartmentOverviewDto>();
        public List<ManagedTeamDto> ManagedTeams { get; set; } = new List<ManagedTeamDto>();
        public List<HierarchyNodeDto> Hierarchy { get; set; } = new List<HierarchyNodeDto>();

        // 4. Quick Actions
        public List<QuickActionDto> QuickActions { get; set; } = new List<QuickActionDto>();

        // 5. Project Teams
        public List<ProjectTeamDto> ProjectTeams { get; set; } = new List<ProjectTeamDto>();
    }

    public class QuickActionDto
    {
        public string ActionName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty; // e.g., "AddNewManager", "CreateDepartment", "InviteMember"
    }

    public class ManagedTeamDto
    {
        public string TeamId { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string TeamLeaderName { get; set; } = string.Empty;
    }

    public class HierarchyNodeDto
    {
        public string RoleName { get; set; } = string.Empty;
        public int NumberOfEmployees { get; set; }
    }
}
