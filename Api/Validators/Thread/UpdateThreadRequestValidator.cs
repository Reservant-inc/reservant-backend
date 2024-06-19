using FluentValidation;
using Reservant.Api.Models.Dtos.Thread;

namespace Reservant.Api.Validators.Thread;
public class UpdateThreadRequestValidator : AbstractValidator<UpdateThreadRequest>
{
    public UpdateThreadRequestValidator()
    {
        RuleFor(t => t.Title)
            .NotEmpty()
            .WithMessage("Title cannot be empty.");
    }
}
