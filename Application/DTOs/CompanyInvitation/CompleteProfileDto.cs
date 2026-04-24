using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SyncVerse.Application.DTOs.CompanyInvitation
{
    public class CompleteProfileDto
    {
        [Required]
        public string Token { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public List<string>? Skills { get; set; }
        public IFormFile? ProfilePicture { get; set; }
        public SyncVerse.Domain.Enums.Gender? Gender { get; set; }
    }
}
