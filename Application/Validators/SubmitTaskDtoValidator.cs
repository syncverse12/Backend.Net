using FluentValidation;
using SyncVerse.Application.DTOs.Tasks.Employee;

namespace SyncVerse.Application.Validators
{
    public class SubmitTaskDtoValidator : AbstractValidator<SubmitTaskDto>
    {
        public SubmitTaskDtoValidator()
        {
            RuleFor(x => x)
                .Must(dto => !string.IsNullOrWhiteSpace(dto.SubmissionLink) || 
                             !string.IsNullOrWhiteSpace(dto.SubmissionNotes))
                .WithMessage("Must Add Submission Link Or Notes at least");

            RuleFor(x => x.SubmissionLink)
                .Must(BeAValidUrl)
                .When(x => !string.IsNullOrWhiteSpace(x.SubmissionLink))
                .WithMessage("Invalid Link, Please Upload a Vaild Link");

            RuleFor(x => x.SubmissionNotes)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.SubmissionNotes))
                .WithMessage("Notes Must Not Exceed 1000 Letters");
        }

        private bool BeAValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
