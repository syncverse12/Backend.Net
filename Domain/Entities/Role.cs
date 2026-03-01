using Microsoft.AspNetCore.Identity;

namespace SyncVerse.Domain.Entities
{
    public class Role : IdentityRole
    {
        public string? Description { get; set; }
    }
}
