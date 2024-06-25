using FluentValidation;
namespace Reservant.Api.Validators.Thread;

public class ThreadValidator : AbstractValidator<Models.MessageThread>
{
    public ThreadValidator()
    {
        RuleFor(t => t.Title)
            .NotEmpty()
            .WithMessage("Title cannot be empty.");
    }
}