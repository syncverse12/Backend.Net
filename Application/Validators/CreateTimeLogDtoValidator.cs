using FluentValidation;
using Graduation_Project.Application.DTOs.Tasks.Employee;

namespace Graduation_Project.Application.Validators
{
    public class CreateTimeLogDtoValidator : AbstractValidator<CreateTimeLogDto>
    {
        public CreateTimeLogDtoValidator()
        {
            RuleFor(x => x.StartTime)
                .NotEmpty()
                .WithMessage("Start time is required")
                .LessThan(x => x.EndTime)
                .WithMessage("Start time must be before end time");

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .WithMessage("End time is required")
                .GreaterThan(x => x.StartTime)
                .WithMessage("End time must be after start time");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Notes))
                .WithMessage("Notes must not exceed 500 characters");
        }
    }
}
