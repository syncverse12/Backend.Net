using SyncVerse.API.JwtFeatuers;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Auth;
using SyncVerse.Application.Interfaces.Identity;
using SyncVerse.Domain.Entities;
using Microsoft.AspNetCore.Identity;

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

        // ✅ Register with OTP
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
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return Result<RegisterResponseDto>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

            // Assign default role
            await _userManager.AddToRoleAsync(user, "Employee");

            // Generate OTP
            var otp = _otpService.GenerateOtp();
            user.OtpCodeHash = _otpService.HashOtp(otp);
            user.OtpExpirationDate = DateTime.UtcNow.AddMinutes(10);
            user.OtpFailedAttempts = 0;

            await _userManager.UpdateAsync(user);

            // Send OTP email
            var emailBody = $@"
                <html>
                <body>
                    <h2>Welcome to Project Management System!</h2>
                    <p>Your OTP for email verification is: <strong>{otp}</strong></p>
                    <p>This OTP will expire in 10 minutes.</p>
                </body>
                </html>";

            await _emailService.SendAsync(user.Email!, "Verify Your Email", emailBody);

            return Result<RegisterResponseDto>.Success(new RegisterResponseDto
            {
                UserId = user.Id,
                Email = user.Email!,
                Message = "Registration successful. Please verify your email with the OTP sent.",
                OtpExpiresAt = user.OtpExpirationDate.Value
            });
        }

        // ✅ Verify Email OTP
        public async Task<Result<AuthResponseDto>> VerifyEmailAsync(VerifyEmailDto dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<AuthResponseDto>.Failure("User not found");

            if (user.IsEmailVerified)
                return Result<AuthResponseDto>.Failure("Email already verified");

            if (string.IsNullOrEmpty(user.OtpCodeHash))
                return Result<AuthResponseDto>.Failure("No OTP found. Please request a new one.");

            if (user.OtpExpirationDate < DateTime.UtcNow)
                return Result<AuthResponseDto>.Failure("OTP expired. Please request a new one.");

            if (user.OtpFailedAttempts >= 5)
                return Result<AuthResponseDto>.Failure("Too many failed attempts. Please request a new OTP.");

            if (!_otpService.VerifyOtp(dto.Otp, user.OtpCodeHash))
            {
                user.OtpFailedAttempts++;
                await _userManager.UpdateAsync(user);
                return Result<AuthResponseDto>.Failure($"Invalid OTP. {5 - user.OtpFailedAttempts} attempts remaining.");
            }

            // ✅ Verification successful
            user.IsEmailVerified = true;
            user.EmailConfirmed = true;
            user.OtpCodeHash = null;
            user.OtpExpirationDate = null;
            user.OtpFailedAttempts = 0;

            await _userManager.UpdateAsync(user);

            // Generate JWT
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtHandler.GenerateToken(user, roles);

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                Token = token.Token,
                Expiration = token.Expiration,
                User = new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList()
                },
                Message = "Email verified successfully"
            });
        }

        // ✅ Login
        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Result<AuthResponseDto>.Failure("Invalid email or password");

            // ✅ Check email verification
            if (!user.IsEmailVerified)
                return Result<AuthResponseDto>.Failure("Email not verified. Please verify your email first.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Result<AuthResponseDto>.Failure("Invalid email or password");

            // Generate JWT
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtHandler.GenerateToken(user, roles);

            return Result<AuthResponseDto>.Success(new AuthResponseDto
            {
                Token = token.Token,
                Expiration = token.Expiration,
                User = new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList()
                },
                Message = "Login successful"
            });
        }

        // ✅ Forgot Password
        public async Task<Result<ForgotPasswordResponseDto>> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Result<ForgotPasswordResponseDto>.Failure("If email exists, OTP has been sent");

            if (!user.IsEmailVerified)
                return Result<ForgotPasswordResponseDto>.Failure("Email not verified");

            // Generate OTP
            var otp = _otpService.GenerateOtp();
            user.OtpCodeHash = _otpService.HashOtp(otp);
            user.OtpExpirationDate = DateTime.UtcNow.AddMinutes(10);
            user.OtpFailedAttempts = 0;
            user.IsPasswordResetOtpVerified = false;

            await _userManager.UpdateAsync(user);

            // Send OTP email
            var emailBody = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>Your OTP for password reset is: <strong>{otp}</strong></p>
                    <p>This OTP will expire in 10 minutes.</p>
                    <p>If you didn't request this, please ignore this email.</p>
                </body>
                </html>";

            await _emailService.SendAsync(user.Email!, "Password Reset OTP", emailBody);

            return Result<ForgotPasswordResponseDto>.Success(new ForgotPasswordResponseDto
            {
                UserId = user.Id,
                Message = "OTP sent to your email",
                OtpExpiresAt = user.OtpExpirationDate.Value
            });
        }

        // ✅ Verify Forgot Password OTP
        public async Task<Result<bool>> VerifyForgotPasswordOtpAsync(VerifyOtpDto dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found");

            if (string.IsNullOrEmpty(user.OtpCodeHash))
                return Result<bool>.Failure("No OTP found");

            if (user.OtpExpirationDate < DateTime.UtcNow)
                return Result<bool>.Failure("OTP expired");

            if (user.OtpFailedAttempts >= 5)
                return Result<bool>.Failure("Too many failed attempts");

            if (!_otpService.VerifyOtp(dto.Otp, user.OtpCodeHash))
            {
                user.OtpFailedAttempts++;
                await _userManager.UpdateAsync(user);
                return Result<bool>.Failure($"Invalid OTP. {5 - user.OtpFailedAttempts} attempts remaining.");
            }

            // ✅ OTP verified - allow password reset
            user.IsPasswordResetOtpVerified = true;
            user.OtpFailedAttempts = 0;
            await _userManager.UpdateAsync(user);

            return Result<bool>.Success(true, "OTP verified. You can now reset your password.");
        }

        // ✅ Reset Password
        public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found");

            if (!user.IsPasswordResetOtpVerified)
                return Result<bool>.Failure("OTP not verified. Please verify OTP first.");

            if (user.OtpExpirationDate < DateTime.UtcNow)
                return Result<bool>.Failure("OTP session expired. Please request a new OTP.");

            // Reset password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
                return Result<bool>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

            // Clear OTP data
            user.OtpCodeHash = null;
            user.OtpExpirationDate = null;
            user.OtpFailedAttempts = 0;
            user.IsPasswordResetOtpVerified = false;

            await _userManager.UpdateAsync(user);

            return Result<bool>.Success(true, "Password reset successfully");
        }

        // ✅ Resend OTP
        public async Task<Result<string>> ResendVerificationOtpAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result<string>.Failure("User not found");

            if (user.IsEmailVerified)
                return Result<string>.Failure("Email already verified");

            // Generate new OTP
            var otp = _otpService.GenerateOtp();
            user.OtpCodeHash = _otpService.HashOtp(otp);
            user.OtpExpirationDate = DateTime.UtcNow.AddMinutes(10);
            user.OtpFailedAttempts = 0;

            await _userManager.UpdateAsync(user);

            // Send OTP email
            var emailBody = $@"
                <html>
                <body>
                    <h2>Email Verification</h2>
                    <p>Your new OTP is: <strong>{otp}</strong></p>
                    <p>This OTP will expire in 10 minutes.</p>
                </body>
                </html>";

            await _emailService.SendAsync(user.Email!, "Email Verification OTP", emailBody);

            return Result<string>.Success("OTP resent successfully");
        }
    }
}