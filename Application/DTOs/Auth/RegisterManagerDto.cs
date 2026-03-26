using System.ComponentModel.DataAnnotations;

namespace SyncVerse.Application.DTOs.Auth
{
    public class RegisterManagerDto
    {
        [Required]
        public string FirstName { get; set; } = null!;
        [Required]
        public string LastName { get; set; } = null!;
        [Required, EmailAddress]
        public string Email { get; set; } = null!;
        [Required, MinLength(6)]
        public string Password { get; set; } = null!;
        [Required]
        public string WorkspaceName { get; set; } = null!;
        [Required]
        public string Industry { get; set; } = null!;
    }
}
