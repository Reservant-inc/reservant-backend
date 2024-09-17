using FluentValidation;
namespace Reservant.Api.Validators.Thread;

/// <summary>
/// Validator for MessageThread
/// </summary>
public class ThreadValidator : AbstractValidator<Models.MessageThread>
{
    /// <inheritdoc/>
    public ThreadValidator()
    {
        RuleFor(t => t.Title)
            .NotEmpty()
            .WithMessage("Title cannot be empty.");
    }
}