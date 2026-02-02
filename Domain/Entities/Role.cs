using Microsoft.AspNetCore.Identity;

namespace Graduation_Project.Domain.Entities
{
    public class Role : IdentityRole
    {
        public string? Description { get; set; }
    }
}
