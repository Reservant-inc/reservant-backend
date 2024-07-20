using FluentValidation;
namespace Reservant.Api.Validators.Message;

public class MessageValidator : AbstractValidator<Models.Message>
{
    public MessageValidator()
    {
        RuleFor(m => m.Contents)
            .NotEmpty()
            .WithMessage("Contents cannot be empty.");
    }
}