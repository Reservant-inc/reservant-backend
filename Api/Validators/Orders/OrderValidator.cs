using FluentValidation;

namespace Reservant.Api.Validators.Orders;

/// <summary>
/// Validator for Order
/// </summary>
public class OrderValidator : AbstractValidator<Models.Order>
{
    /// <inheritdoc />
    public OrderValidator()
    {
        RuleFor(o => o.Note)
            .MaximumLength(100)
            .NotEmpty()
            .When(o => o.Note is not null);
    }
}
