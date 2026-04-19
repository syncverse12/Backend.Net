namespace SyncVerse.Application.DTOs.Workspaces
{
    public class WorkspaceResponseDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CreatedByUserId { get; set; } = null!;
        public string CreatedByUserName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string OrgCode { get; set; } = null!;
    }
}
