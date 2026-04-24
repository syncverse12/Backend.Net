using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Auth;


namespace SyncVerse.Application.Interfaces.Identity
{
    public interface IAuthService
    {
        // ✅ Existing
        Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
        
        // ✅ NEW: Manager Registration with Workspace Creation
        Task<Result<AuthResponseDto>> RegisterManagerAsync(RegisterManagerDto dto);

        // ✅ NEW: OTP-based Registration
        Task<Result<RegisterResponseDto>> RegisterAsync(RegisterDto dto);
        Task<Result<AuthResponseDto>> VerifyEmailAsync(VerifyEmailDto dto, string userId);
        
        // ✅ NEW: Forgot Password Flow
        Task<Result<ForgotPasswordResponseDto>> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<Result<bool>> VerifyForgotPasswordOtpAsync(VerifyOtpDto dto, string userId);
        Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto dto, string userId);
        
        // ✅ NEW: Resend OTP
        Task<Result<string>> ResendVerificationOtpAsync(string email);
    }
}