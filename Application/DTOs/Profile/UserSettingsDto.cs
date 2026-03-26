namespace SyncVerse.Application.DTOs.Profile
{
    public class UserSettingsDto
    {
        public string Theme { get; set; } = "System"; 
        public string Language { get; set; } = "en"; 
        public string TimeZoneId { get; set; } = "UTC";

        public bool EnableEmailNotifications { get; set; }
        public bool EnableInAppNotifications { get; set; }
        public bool NotifyOnTaskAssignment { get; set; }
        public int TaskReminderAdvanceHours { get; set; }

        public string AvailabilityStatus { get; set; } = "Active"; 
        public string? StatusMessage { get; set; }
        public bool ShowEmailToTeam { get; set; }
    }
}