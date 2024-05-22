using FluentValidation;

namespace Reservant.Api.Validators.Order;

public class OrderValidator : AbstractValidator<Models.Order>
{
    /// <inheritdoc />
    public OrderValidator()
    {
        RuleFor(o => o.Note)
            .NotEmpty()
            .When(o => o.Note is not null);

    }
}
