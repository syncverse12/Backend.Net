using Graduation_Project.Domain.Enums;

public class UpdateTeamMemberRoleDto
{
    public string TeamMemberId { get; set; } = null!;
    public TeamRole Role { get; set; }
}
