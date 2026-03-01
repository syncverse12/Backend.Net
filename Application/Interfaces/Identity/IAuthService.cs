using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Auth;

namespace Graduation_Project.Application.Interfaces.Identity
{
    public interface IAuthService
    {
        // ✅ Existing
        Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
        
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