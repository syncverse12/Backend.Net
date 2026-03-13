using SyncVerse.Domain.Entities;
using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.DTOs.Team
{
    public class TeamResponseDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public TeamSpecialization Specialization { get; set; }
        public Department Department { get; set; }
        public string DepartmentDisplay { get; set; } = null!; 
        public string ManagerName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int MembersCount { get; set; }
    }
}
