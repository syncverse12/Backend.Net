using Graduation_Project.Domain.Common;
using Graduation_Project.Domain.Models;

namespace Graduation_Project.Domain.Entities
{
    public class Workspace : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CreatedByUserId { get; set; } = null!;
        public User CreatedByUser { get; set; } = null!;
    }
}
