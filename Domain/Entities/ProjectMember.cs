using Graduation_Project.Domain.Common;

public class ProjectMember : BaseEntity
{
    public string ProjectId { get; set; } = null!;
    public string UserId { get; set; }=null!;

    public DateTime JoinedAt { get; set; }
}
