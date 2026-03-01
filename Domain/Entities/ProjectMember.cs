using SyncVerse.Domain.Common;
using SyncVerse.Domain.Enums;

public class ProjectMember : BaseEntity
{
    public string ProjectId { get; set; } = null!;
    public string UserId { get; set; }=null!;
    public ProjectRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}
