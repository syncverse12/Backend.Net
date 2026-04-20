namespace SyncVerse.Application.Interfaces.Identity
{
    public interface IOtpService
    {
        string GenerateOtp();
        string HashOtp(string otp);
        bool VerifyOtp(string otp, string hash);

        /// <summary>
        /// Generates an OTP and sends it to the specified email address.
        /// </summary>
        /// <param name="email">The email address to send the OTP to.</param>
        /// <returns>True if sent successfully, otherwise false.</returns>
        Task<bool> GenerateAndSendOtpAsync(string email);
    }
}
