using Graduation_Project.Domain.Enums;

public class InviteTeamMemberDto
{
    public string ProjectId { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
    public TeamRole Role { get; set; }
}
