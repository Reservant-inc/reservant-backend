using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Data;

namespace Reservant.Api.Validators.Event
{
    public class EventValidator : AbstractValidator<Models.Event>
    {
        public EventValidator(ApiDbContext context, UserManager<Models.User> userManager) {
            RuleFor(e => e.RestaurantId)
                .NotNull()
                .RestaurantExists(context);

            RuleFor(e => e.Restaurant)
                .NotNull();

            RuleFor(e => e.MustJoinUntil)
                .NotNull()
                .DateTimeInFuture();

            RuleFor(e => e.Time)
                .NotNull()
                .DateTimeInFuture();

            RuleFor(e => e.CreatorId)
                .NotNull()
                .CustomerExists(userManager);

            RuleFor(e => e.Creator)
                .NotNull();
        }
    }
}
