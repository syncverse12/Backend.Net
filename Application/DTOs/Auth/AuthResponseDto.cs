namespace SyncVerse.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public UserResponseDto User { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? OrgCode { get; set; }
    }
}
