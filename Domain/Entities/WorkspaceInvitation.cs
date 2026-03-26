using System;
using SyncVerse.Domain.Common;
using SyncVerse.Domain.Enums;

namespace SyncVerse.Domain.Entities
{
    public class WorkspaceInvitation : BaseEntity
    {
        public string Email { get; set; } = null!;
        public string WorkspaceId { get; set; } = null!;
        public Workspace Workspace { get; set; } = null!;
        
        public Department Department { get; set; }
        public SeniorityLevel SeniorityLevel { get; set; }
        public string RoleToAssign { get; set; } = "Employee"; 
        
        public string InvitationToken { get; set; } = null!;
        public string InvitedByManagerId { get; set; } = null!;
        public User InvitedByManager { get; set; } = null!;
        
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
        public DateTime ExpiresAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }
}