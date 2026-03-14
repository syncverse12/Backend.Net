using System.ComponentModel.DataAnnotations;

namespace SyncVerse.Application.DTOs.Profile
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = null!;
        
        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string NewPassword { get; set; } = null!;
    }
}
