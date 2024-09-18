using FluentValidation;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Event;

namespace Reservant.Api.Validators.Event;

/// <summary>
/// Validator for CreateEventRequest
/// </summary>
public class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
    /// <inheritdoc />
    public CreateEventRequestValidator(ApiDbContext context) {
        RuleFor(e => e.Time)
            .NotNull()
            .DateTimeInFuture();

        RuleFor(e => e.MustJoinUntil)
            .NotNull()
            .DateTimeInFuture();

        RuleFor(e => e.RestaurantId)
            .RestaurantExists(context);

        RuleFor(e => e.Description)
            .Length(0, 200)
            .When(e => e.Description is not null);
    }
}
