namespace SyncVerse.Application.DTOs.UserManagement
{
    public class UserListDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string SeniorityLevel { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsLockedOut { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
