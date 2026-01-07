namespace Graduation_Project.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public bool IsAuthenticated { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime? ExpiresOn { get; set; }
    }
}