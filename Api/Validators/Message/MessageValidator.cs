using FluentValidation;
namespace Reservant.Api.Validators.Message;

/// <summary>
/// Validator for Message
/// </summary>
public class MessageValidator : AbstractValidator<Models.Message>
{
    /// <inheritdoc/>
    public MessageValidator()
    {
        RuleFor(m => m.Contents)
            .NotEmpty()
            .WithMessage("Contents cannot be empty.")
            .MaximumLength(200)
            .WithMessage("Contents cannot be longer than 200 characters.");
    }
}