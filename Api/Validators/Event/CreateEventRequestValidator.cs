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
                .When(e => e.Time>DateTime.Now);

            RuleFor(e => e.MustJoinUntil)
                .NotNull()
                .When(e => e.MustJoinUntil > DateTime.Now && e.MustJoinUntil <= e.Time);

            RuleFor(e => e.RestaurantId)
                .NotNull()
                .RestaurantExists(context);
        }
    }
}
