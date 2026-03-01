using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.DTOs.Project.Manager
{
    public class InviteEmployeeDto
    {
        public string EmployeeId { get; set; } = null!;
        public ProjectRole Role { get; set; } 
        public InvitationType Type { get; set; }
    }

}
