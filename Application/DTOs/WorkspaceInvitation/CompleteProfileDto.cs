using Microsoft.AspNetCore.Http; // For IFormFile
using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.DTOs.WorkspaceInvitation
{
    public class CompleteProfileDto
    {
        //invite token
        public string Token { get; set; } = null!;
        
        public string? PhoneNumber { get; set; } 
        
        // ✅ Profile Data
        public string? Skills { get; set; }
        
        public string? Address { get; set; }
        
        public IFormFile? ProfilePicture { get; set; }
    }
}