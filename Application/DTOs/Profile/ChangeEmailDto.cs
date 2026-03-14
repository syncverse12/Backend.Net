using System.ComponentModel.DataAnnotations;

namespace SyncVerse.Application.DTOs.Profile
{
    public class ChangeEmailDto
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; } = null!;
    }
}
