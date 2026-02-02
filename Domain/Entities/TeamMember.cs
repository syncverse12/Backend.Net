using Graduation_Project.Domain.Common;
using Graduation_Project.Domain.Enums;
using Graduation_Project.Domain.Entities;

namespace Graduation_Project.Domain.Entities
{
    public class TeamMember : BaseEntity
    {
        public string ProjectId { get; set; } = null!;
        public Project Project { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        public TeamRole Role { get; set; } = TeamRole.Contributor;

        public bool IsActive { get; set; } = false; 
    }
}
