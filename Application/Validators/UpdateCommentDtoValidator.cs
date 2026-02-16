using FluentValidation;
using Graduation_Project.Application.DTOs.Tasks.Comments;

namespace Graduation_Project.Application.Validators
{
    public class UpdateCommentDtoValidator : AbstractValidator<UpdateCommentDto>
    {
        public UpdateCommentDtoValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Comment content is required")
                .MaximumLength(2000)
                .WithMessage("Comment content must not exceed 2000 characters");
        }
    }
}
