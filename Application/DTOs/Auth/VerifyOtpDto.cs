using System.ComponentModel.DataAnnotations;

namespace SyncVerse.Application.DTOs.Auth
{
    public class VerifyOtpDto
    {
        [Required(ErrorMessage = "OTP is required")]
        [StringLength(4, MinimumLength = 4)]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "OTP must be 4 digits")]
        public string Otp { get; set; } = null!;
    }
}
