using SyncVerse.Domain.Enums;

public class InviteTeamMemberDto
{
    public string ProjectId { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
    public ProjectRole Role { get; set; }
}
