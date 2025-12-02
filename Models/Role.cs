using Microsoft.AspNetCore.Identity;

namespace Graduation_Project.Models
{
    public class Role : IdentityRole
    {
        public string? Description { get; set; }
    }
}
