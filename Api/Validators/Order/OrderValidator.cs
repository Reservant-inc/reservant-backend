using FluentValidation;

namespace Reservant.Api.Validators.Order;

public class OrderValidator : AbstractValidator<Models.Order>
{
    /// <inheritdoc />
    public OrderValidator()
    {
        RuleFor(o => o.VisitId)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Visit id must be non-negative")
            .WithErrorCode(ErrorCodes.VisitId);

        RuleFor(o => o.Note)
            .NotNull()
            .WithMessage("Value cannot be null");
        
    }
}