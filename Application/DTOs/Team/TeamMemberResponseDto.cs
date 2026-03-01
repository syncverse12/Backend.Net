using SyncVerse.Domain.Enums;

public class TeamMemberResponseDto
{
    public string TeamMemberId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
    public ProjectRole Role { get; set; }
    public bool IsActive { get; set; }
}
