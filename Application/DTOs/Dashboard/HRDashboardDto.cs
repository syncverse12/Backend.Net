namespace SyncVerse.Application.DTOs.Dashboard
{
    public class HRDashboardDto
    {
        public List<DepartmentOverviewDto> EmployeeOverview { get; set; } = new List<DepartmentOverviewDto>();
        public InvitationTrackingDto InvitationsTracking { get; set; } = new InvitationTrackingDto();
        public List<DepartmentPerformanceDto> DepartmentPerformance { get; set; } = new List<DepartmentPerformanceDto>();
    }

    public class DepartmentOverviewDto
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
    }

    public class InvitationTrackingDto
    {
        public int TotalSent { get; set; }
        public int Accepted { get; set; }
        public int Pending { get; set; }
        public int Expired { get; set; }
        public int Cancelled { get; set; }
        public List<RecentInvitationDto> RecentInvitations { get; set; } = new List<RecentInvitationDto>();
    }

    public class RecentInvitationDto
    {
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class DepartmentPerformanceDto
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int ActiveTasks { get; set; }
        public int CompletedTasksThisMonth { get; set; }
    }
}