using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Data;

namespace Reservant.Api.Validators.Event
{
    public class EventValidator : AbstractValidator<Models.Event>
    {
        public EventValidator(ApiDbContext context, UserManager<Models.User> userManager)
        {
            RuleFor(x => x.Description)
                .Length(0, 200)
                .When(x => x.Description is not null);
        }
    }
}
