using SyncVerse.API.JwtFeatuers;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Auth;
using SyncVerse.Application.Interfaces.Identity;
using SyncVerse.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using SyncVerse.Domain.Enums;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SyncVerse.Application.Services.Identity
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtHandler _jwtHandler;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            JwtHandler jwtHandler,
            IOtpService otpService,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtHandler = jwtHandler;
            _otpService = otpService;
            _emailService = emailService;
        }

        // ✅ 1. Register
        public async Task<Result<RegisterResponseDto>> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return Result<RegisterResponseDto>.Failure("Email already registered");

            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                SeniorityLevel = SeniorityLevel.Intern,
                Department = Department.Engineering
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return Result<RegisterResponseDto>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, "Employee");

            var otp = _otpService.GenerateOtp();
            user.OtpCodeHash = _otpService.HashOtp(otp);
            user.OtpExpirationDate = DateTime.UtcNow.AddMinutes(10);
            await _userManager.UpdateAsync(user);

            var emailBody = GetFormalHtmlTemplate("Verify Your Email", $"Thank you for registering SyncVerse, {dto.FirstName}!", otp);
            await _emailService.SendAsync(user.Email!, "Account Verification", emailBody);

            return Result<RegisterResponseDto>.Success(new RegisterResponseDto
            {
                UserId = user.Id,
                Email = user.Email!,
                Message = "Registration successful. Please verify your email.",
                OtpExpiresAt = user.OtpExpirationDate.Value
            });
        }

        // ✅ 2. Forgot Password
        public async Task<Result<ForgotPasswordResponseDto>> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Result<ForgotPasswordResponseDto>.Failure("If the email exists, an OTP has been sent.");

            var otp = _otpService.GenerateOtp();
            user.OtpCodeHash = _otpService.HashOtp(otp);
            user.OtpExpirationDate = DateTime.UtcNow.AddMinutes(10);
            user.IsPasswordResetOtpVerified = false;
            await _userManager.UpdateAsync(user);

            var emailBody = GetFormalHtmlTemplate("Password Reset", "We received a request to reset your password.", otp);
            await _emailService.SendAsync(user.Email!, "SyncVerse Password Reset", emailBody);

            return Result<ForgotPasswordResponseDto>.Success(new ForgotPasswordResponseDto
            {
                UserId = user.Id,
                Message = "Password reset OTP sent successfully.",
                OtpExpiresAt = user.OtpExpirationDate.Value
            });
        }

        // ✅ 3. Resend OTP
        public async Task<Result<string>> ResendVerificationOtpAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.IsEmailVerified) return Result<string>.Failure("Invalid request.");

            var otp = _otpService.GenerateOtp();
            user.OtpCodeHash = _otpService.HashOtp(otp);
            user.OtpExpirationDate = DateTime.UtcNow.AddMinutes(10);
            await _userManager.UpdateAsync(user);

            var emailBody = GetFormalHtmlTemplate("New Verification Code", "Here is your new verification code to continue registration.", otp);
            await _emailService.SendAsync(user.Email!, "Your New Verification Code", emailBody);

            return Result<string>.Success("A new OTP has been sent to your email.");
        }

        // 🛠️ Minimalist Formal HTML Template (Clean Black/White/Grey)
        private string GetFormalHtmlTemplate(string title, string message, string code)
        {
            return $@"
            <div style='background-color: #1a1a1a; color: #ffffff; font-family: Arial, sans-serif; padding: 40px; text-align: center; border-radius: 8px;'>
                <h2 style='color: #ffffff; margin-bottom: 20px; font-weight: normal;'>{title}</h2>
                <p style='font-size: 16px; color: #cccccc;'>{message}</p>
                <p style='font-size: 14px; margin-top: 30px; color: #888888;'>Your Verification Code:</p>
                <div style='background-color: #2d2d2d; color: #ffffff; font-size: 36px; font-weight: bold; letter-spacing: 4px; padding: 20px 40px; margin: 20px auto; display: inline-block; border-radius: 4px; border: 1px solid #444444;'>
                    {code}
                </div>
                <p style='font-size: 12px; color: #666666; margin-top: 25px;'>This code will expire in 10 minutes.<br/>For security reasons, do not share this code with anyone.</p>
                <hr style='border: none; border-top: 1px solid #333333; margin: 30px 0;'>
                <p style='font-size: 11px; color: #555555;'>© 2026 SyncVerse</p>
            </div>";
        }

        // ✅ 4. Verify Email
        public async Task<Result<AuthResponseDto>> VerifyEmailAsync(VerifyEmailDto dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Result<AuthResponseDto>.Failure("User not found");
            if (user.IsEmailVerified) return Result<AuthResponseDto>.Failure("Email already verified");

            if (!_otpService.VerifyOtp(dto.Otp, user.OtpCodeHash!))
            {
                user.OtpFailedAttempts++;
                await _userManager.UpdateAsync(user);
                return Result<AuthResponseDto>.Failure("Invalid OTP.");
            }

            user.IsEmailVerified = true;
            user.EmailConfirmed = true;
            user.OtpCodeHash = null;
            user.OtpExpirationDate = null;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtHandler.GenerateToken(user, roles);

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                Token = token.Token,
                Expiration = token.Expiration,
                User = new UserResponseDto { Id = user.Id, Email = user.Email!, FirstName = user.FirstName, LastName = user.LastName, Roles = roles.ToList() },
                Message = "Email verified successfully"
            });
        }

        // ✅ 5. Login
        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !user.IsEmailVerified)
                return Result<AuthResponseDto>.Failure("Email not verified or invalid credentials.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded) return Result<AuthResponseDto>.Failure("Invalid email or password");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtHandler.GenerateToken(user, roles);

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                Token = token.Token,
                Expiration = token.Expiration,
                User = new UserResponseDto { Id = user.Id, Email = user.Email!, FirstName = user.FirstName, LastName = user.LastName, Roles = roles.ToList() },
                Message = "Login successful"
            });
        }

        // ✅ 6. Verify Forgot Password OTP
        public async Task<Result<bool>> VerifyForgotPasswordOtpAsync(VerifyOtpDto dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.OtpCodeHash)) return Result<bool>.Failure("Invalid request.");

            if (!_otpService.VerifyOtp(dto.Otp, user.OtpCodeHash)) return Result<bool>.Failure("Invalid OTP.");

            user.IsPasswordResetOtpVerified = true;
            await _userManager.UpdateAsync(user);
            return Result<bool>.Success(true, "OTP verified.");
        }

        // ✅ 7. Reset Password
        public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsPasswordResetOtpVerified) return Result<bool>.Failure("Unauthorized.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded) return Result<bool>.Failure("Failed to reset password.");

            user.OtpCodeHash = null;
            user.IsPasswordResetOtpVerified = false;
            await _userManager.UpdateAsync(user);

            return Result<bool>.Success(true, "Password reset successfully.");
        }
    }
}