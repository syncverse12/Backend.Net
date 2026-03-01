using FluentValidation;
using SyncVerse.Application.DTOs.Auth;

namespace SyncVerse.Application.Validators
{
    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");
        }
    }
}