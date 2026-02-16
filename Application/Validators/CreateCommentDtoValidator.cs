using FluentValidation;
using Graduation_Project.Application.DTOs.Tasks.Comments;

namespace Graduation_Project.Application.Validators
{
    public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentDtoValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Comment content is required")
                .MaximumLength(2000)
                .WithMessage("Comment content must not exceed 2000 characters");
        }
    }
}
