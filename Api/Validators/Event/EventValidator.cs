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
        RuleFor(x => x.Description)
            .Length(0, 200)
            .When(x => x.Description is not null);
    }
}
