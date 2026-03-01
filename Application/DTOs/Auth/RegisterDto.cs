using System.ComponentModel.DataAnnotations;

namespace SyncVerse.Application.DTOs.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$",
            ErrorMessage = "Password must contain uppercase, lowercase, number, and special character")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = null!;
    }
}