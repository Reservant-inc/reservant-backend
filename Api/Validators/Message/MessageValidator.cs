using FluentValidation;
namespace Reservant.Api.Validators.Message;

public class MessageValidator : AbstractValidator<Models.Message>
{
    public MessageValidator()
    {
        RuleFor(m => m.Contents)
            .NotEmpty()
            .WithMessage("Contents cannot be empty.")
            .MaximumLength(200)
            .WithMessage("Contents cannot be longer than 200 characters.");
    }
}