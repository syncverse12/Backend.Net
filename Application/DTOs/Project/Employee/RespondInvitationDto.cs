namespace SyncVerse.Application.DTOs.Project.Employee
{
    public class RespondInvitationDto
    {
        public InvitationStatus Status { get; set; } 
        public string? RejectionReason { get; set; }
    }

}
