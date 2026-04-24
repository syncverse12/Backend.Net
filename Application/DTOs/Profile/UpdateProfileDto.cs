using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace SyncVerse.Application.DTOs.Profile
{
    public class UpdateProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public List<string>? Skills { get; set; }
        public IFormFile? ProfilePicture { get; set; }
        public string? Gender { get; set; }
    }
}
