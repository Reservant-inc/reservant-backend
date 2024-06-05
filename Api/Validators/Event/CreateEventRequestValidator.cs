using FluentValidation;
using Reservant.Api.Data;
using Reservant.Api.Models.Dtos.Event;

namespace Reservant.Api.Validators.Event
{
    public class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
    {

        public CreateEventRequestValidator(ApiDbContext context) {
            RuleFor(e => e.Time)
                .NotNull()
                .DateTimeInFuture();

            RuleFor(e => e.MustJoinUntil)
                .NotNull()
                .DateTimeInFuture();

            RuleFor(e => e.RestaurantId)
                .NotNull()
                .RestaurantExists(context);
        }
    }
}
