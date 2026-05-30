using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncVerse.Domain.Entities
{
    public class UserSettings
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        // 1. UI Preferences
        public string Theme { get; set; } = "System"; // Light, Dark, System
        public string TimeZoneId { get; set; } = "UTC";

        // 2. Notification Preferences
        public bool EnableEmailNotifications { get; set; } = true;
        public bool EnableInAppNotifications { get; set; } = true;
        public bool NotifyOnTaskAssignment { get; set; } = true;
        public int TaskReminderAdvanceHours { get; set; } = 24;

        // 3. Privacy & Status
        public string AvailabilityStatus { get; set; } = "Active"; // Active, Busy, Offline, DoNotDisturb
        public string? StatusMessage { get; set; }
        public bool ShowEmailToTeam { get; set; } = true;
    }
}