using SyncVerse.Domain.Enums;
using SyncVerse.Domain.Entities;

namespace SyncVerse.Application.DTOs.Profile
{
    public class UserTeamDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Department Department { get; set; }
        public string DepartmentDisplay => Department.ToString();
        public TeamSpecialization Specialization { get; set; }
        public int MembersCount { get; set; }
    }
}
