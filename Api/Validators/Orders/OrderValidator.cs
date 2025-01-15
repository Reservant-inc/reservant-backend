using FluentValidation;
using Reservant.Api.Models;

namespace Reservant.Api.Validators.Orders;

/// <summary>
/// Validator for Order
/// </summary>
public class OrderValidator : AbstractValidator<Order>
{
    /// <inheritdoc />
    public OrderValidator()
    {
        RuleFor(o => o.Note)
            .MaximumLength(Order.MaxNoteLength)
            .NotEmpty()
            .When(o => o.Note is not null);

        RuleFor(o => o.OrderItems)
            .NotEmpty();
    }
}
