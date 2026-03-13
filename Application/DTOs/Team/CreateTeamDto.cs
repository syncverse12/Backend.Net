using SyncVerse.Domain.Entities;
using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.DTOs.Team
{
    public class CreateTeamDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public TeamSpecialization Specialization { get; set; }
        
        public Department Department { get; set; } = Department.Engineering;
    }
}
