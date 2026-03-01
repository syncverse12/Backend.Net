namespace SyncVerse.Application.Interfaces.Identity
{
    public interface IOtpService
    {
        string GenerateOtp();
        string HashOtp(string otp);
        bool VerifyOtp(string otp, string hash);
    }
}
