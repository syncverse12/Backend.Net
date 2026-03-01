namespace SyncVerse.Application.DTOs.Auth
{
    public class ForgotPasswordResponseDto
    {
        public string UserId { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime OtpExpiresAt { get; set; }
    }
}
