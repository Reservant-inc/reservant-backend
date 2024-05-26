using FluentValidation;

namespace Reservant.Api.Validators.Order;
    /// <summary>
    /// Validator for Order
    /// </summary>
public class OrderValidator : AbstractValidator<Models.Order>
{
    /// <inheritdoc />
    public OrderValidator()
    {
        RuleFor(o => o.VisitId)
            .NotNull()
            .GreaterThan(0);

        RuleFor(o => o.Note)
            .MaximumLength(100);

        RuleFor(o => o.EmployeeId)
            .NotEmpty()
            .When(o => o.EmployeeId != null);

        RuleFor(o => o.OrderItems)
            .NotEmpty();
    }
}