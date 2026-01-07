using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.Application.DTOs.Auth
{
    public class RegisterRequestDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Compare("Password")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
