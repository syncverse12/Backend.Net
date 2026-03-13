using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.DTOs.WorkspaceInvitation
{
    public class CompleteProfileDto
    {
        public string Token { get; set; } = null!;
        public string? PhoneNumber { get; set; } 
    }
}