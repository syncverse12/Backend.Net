using SyncVerse.Domain.Enums;

public class UpdateTeamMemberRoleDto
{
    public string TeamMemberId { get; set; } = null!;
    public ProjectRole Role { get; set; }

}
