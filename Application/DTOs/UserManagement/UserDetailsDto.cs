namespace SyncVerse.Application.DTOs.UserManagement
{
    public class UserDetailsDto : UserListDto
    {
        public string DisplayName { get; set; } = string.Empty;
        public string OrgCode { get; set; } = string.Empty;
        public string TeamId { get; set; } = string.Empty;
        public int TeamSize { get; set; }
        public string Gender { get; set; } = "Unknown";

        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Address { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public bool IsEmailVerified { get; set; }
    }
}
