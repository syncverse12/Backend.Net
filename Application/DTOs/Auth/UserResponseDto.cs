using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.DTOs.Auth
{
    public class UserResponseDto
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        
        // ✅ New Info Fields
        public string? PhoneNumber { get; set; }
        public List<string>? Skills { get; set; } 
        public string? Address { get; set; }
        public string? ProfilePictureUrl { get; set; }
        
        public Department Department { get; set; }
        public SeniorityLevel SeniorityLevel { get; set; }
        public List<string> Roles { get; set; } = new();

        public string? WorkspaceId { get; set; }
    }
}
