using Graduation_Project.Domain.Common;

public class ProjectInvitation : BaseEntity
{
    public string ProjectId { get; set; } = null!;
    public string EmployeeId { get; set; } = null!;

    public string SentByManagerId { get; set; }= null!;

    public InvitationType Type { get; set; } 
    public InvitationStatus Status { get; set; } 

    public string? RejectionReason { get; set; }

    public DateTime SentAt { get; set; }
}
