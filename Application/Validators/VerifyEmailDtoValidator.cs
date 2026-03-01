using FluentValidation;
using Graduation_Project.Application.DTOs.Auth;

namespace Graduation_Project.Application.Validators
{
    public class VerifyEmailValidator : AbstractValidator<VerifyEmailDto>
    {
        public VerifyEmailValidator()
        {
            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("OTP is required")
                .Length(4).WithMessage("OTP must be 4 digits")
                .Matches(@"^\d{4}$").WithMessage("OTP must be numeric");
        }
    }
}