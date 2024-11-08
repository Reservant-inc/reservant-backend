using FluentValidation;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Events;

namespace Reservant.Api.Validators.Events;

/// <summary>
/// Validator for UpdateEventRequest
/// </summary>
public class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
{
    /// <inheritdoc />
    public UpdateEventRequestValidator(ApiDbContext context)
    {
        RuleFor(e => e.Name)
            .MaximumLength(50)
            .NotEmpty();

        RuleFor(e => e.MustJoinUntil)
            .LessThan(e => e.Time)
            .WithErrorCode(ErrorCodes.MustJoinUntilMustBeBeforeEventTime)
            .WithMessage("MustJoinUntil must be before the event time");

        RuleFor(e => e.RestaurantId)
            .RestaurantExists(context);

        RuleFor(e => e.Description)
            .MaximumLength(200);
    }
}
