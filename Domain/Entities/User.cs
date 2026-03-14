using Microsoft.AspNetCore.Identity;
using SyncVerse.Domain.Enums;

namespace SyncVerse.Domain.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        public SeniorityLevel SeniorityLevel { get; set; } 
        public Department Department { get; set; }

        // ✅ NEW: Profile Expansion Fields
        public string? ProfilePictureUrl { get; set; }
        public string? Address { get; set; }
        public List<string> Skills { get; set; } = new List<string>(); 

        // ✅ NEW: OTP Fields
        public string? OtpCodeHash { get; set; }
        public DateTime? OtpExpirationDate { get; set; }
        public int OtpFailedAttempts { get; set; } = 0;
        
        // ✅ NEW: Email Verification
        public bool IsEmailVerified { get; set; } = false;
        
        // ✅ NEW: Password Reset Flow
        public bool IsPasswordResetOtpVerified { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties (existing)
        public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
