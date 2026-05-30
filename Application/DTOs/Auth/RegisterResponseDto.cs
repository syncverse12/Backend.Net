namespace SyncVerse.Application.DTOs.Auth
{
    public class RegisterResponseDto
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime OtpExpiresAt { get; set; }
        public string? WorkspaceId { get; set; }
        public string? OrgCode { get; set; }
    }
}
