namespace SyncVerse.Application.DTOs.Workspaces
{
    public class WorkspaceSessionDto
    {
        public string OrgCode { get; set; } = string.Empty;
        public string JoinCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
