using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.Application.DTOs.Auth
{
    public class RegisterRequestDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string ConfirmPassword { get; set; } = null!;
    }
}
