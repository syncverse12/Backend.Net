using SyncVerse.Domain.Entities;
using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.DTOs.Team
{
    public class UpdateTeamDto
    {
        public string TeamId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public TeamSpecialization Specialization { get; set; }
        public Department Department { get; set; }
    }
}
