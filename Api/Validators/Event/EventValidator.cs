using FluentValidation;

namespace Reservant.Api.Validators.Event;

/// <summary>
/// Validator for Event
/// </summary>
public class EventValidator : AbstractValidator<Models.Event>
{
    /// <inheritdoc />
    public EventValidator()
    {
        RuleFor(e => e.Name)
            .NotEmpty()
            .IsValidName();

        RuleFor(x => x.Description)
            .Length(0, 200)
            .When(x => x.Description is not null);

        RuleFor(x => x.MustJoinUntil)
            .LessThan(x => x.Time)
            .WithErrorCode(ErrorCodes.MustJoinUntilMustBeBeforeEventTime);

        RuleFor(x => x.Time)
            .GreaterThan(x => x.MustJoinUntil)
            .WithErrorCode(ErrorCodes.MustJoinUntilMustBeBeforeEventTime);
    }
}
