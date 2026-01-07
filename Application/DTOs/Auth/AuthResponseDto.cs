namespace Graduation_Project.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public bool IsAuthenticated { get; set; }
        public string? Token { get; set; }

        public DateTime? ExpiresOn { get; set; }
        public string? Message { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}
