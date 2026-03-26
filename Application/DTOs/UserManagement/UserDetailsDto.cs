namespace SyncVerse.Application.DTOs.UserManagement
{
    public class UserDetailsDto : UserListDto
    {
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Address { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public bool IsEmailVerified { get; set; }
    }
}
