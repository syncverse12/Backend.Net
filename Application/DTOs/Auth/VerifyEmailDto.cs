using System.ComponentModel.DataAnnotations;

namespace SyncVerse.Application.DTOs.Auth
{
    public class VerifyEmailDto
    {
        [Required(ErrorMessage = "OTP is required")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "OTP must be 4 digits")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "OTP must be numeric")]
        public string Otp { get; set; } = null!;
    }
}
