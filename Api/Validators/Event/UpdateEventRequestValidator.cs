using FluentValidation;
using Reservant.Api.Models.Dtos.Event;

namespace Reservant.Api.Validators.Event;

/// <summary>
/// Validator for UpdateEventRequest
/// </summary>
public class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
{
    /// <inheritdoc />
    public UpdateEventRequestValidator()
    {
        RuleFor(x => x.MustJoinUntil)
            .LessThan(x => x.Time)
            .WithErrorCode(ErrorCodes.MustJoinUntilMustBeBeforeEventTime);

        RuleFor(x => x.Time)
            .GreaterThan(x => x.MustJoinUntil)
            .WithErrorCode(ErrorCodes.MustJoinUntilMustBeBeforeEventTime);
    }
}
