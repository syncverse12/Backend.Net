using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.DTOs.Profile
{
    public class UserProfileDto
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Address { get; set; }
        public List<string> Skills { get; set; } = new();
        
        public Department Department { get; set; }
        public string DepartmentDisplay => Department.ToString();
        
        public SeniorityLevel SeniorityLevel { get; set; }
        public string SeniorityLevelDisplay => SeniorityLevel.ToString();
        
        public List<string> Roles { get; set; } = new();
        public DateTime JoinedDate { get; set; }

        public string? OrgCode { get; set; }
        public Gender? Gender { get; set; }
    }
}