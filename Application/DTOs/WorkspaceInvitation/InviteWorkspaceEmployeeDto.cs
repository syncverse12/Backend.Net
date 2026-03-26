using SyncVerse.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SyncVerse.Application.DTOs.WorkspaceInvitation
{
    public class InviteWorkspaceEmployeeDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;
        public Department Department { get; set; }
        public SeniorityLevel SeniorityLevel { get; set; }
        public string RoleToAssign { get; set; } = "Employee"; 
    }
}