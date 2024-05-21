using FluentValidation;

namespace Reservant.Api.Validators.Order;

public class OrderValidator : AbstractValidator<Models.Order>
{
    /// <inheritdoc />
    public OrderValidator()
    {
        RuleFor(o => o.Note)
            .MinimumLength(0)
            .WithMessage("Note cannot be empty!")
            .WithErrorCode(ErrorCodes.NoteTooShort);

    }
}