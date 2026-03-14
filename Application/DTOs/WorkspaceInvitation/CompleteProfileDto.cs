using Microsoft.AspNetCore.Http; // For IFormFile

namespace SyncVerse.Application.DTOs.WorkspaceInvitation
{
    public class CompleteProfileDto
    {
        //invite token
        public string Token { get; set; } = null!;
        
        public string? PhoneNumber { get; set; } 
        
        // ✅ Profile Data
        public string? Address { get; set; }
        
        public List<string>? Skills { get; set; }
        
        public IFormFile? ProfilePicture { get; set; }
    }
}