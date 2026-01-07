using Microsoft.AspNetCore.Identity;

namespace Graduation_Project.Domain.Models
{
    public class Role : IdentityRole
    {
        public string? Description { get; set; }
    }
}
