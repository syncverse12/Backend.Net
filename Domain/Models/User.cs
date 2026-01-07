using Microsoft.AspNetCore.Identity;

namespace Graduation_Project.Domain.Models
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
