using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.DTOs.UserManagement
{
    public class UpdateUserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Department Department { get; set; }
        public SeniorityLevel SeniorityLevel { get; set; }
    }
}
