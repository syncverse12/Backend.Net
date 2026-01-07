using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
