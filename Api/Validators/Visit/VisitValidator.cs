using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Data;
using Reservant.Api.Models;

namespace Reservant.Api.Validators.Visit;

public class VisitValidator: AbstractValidator<Models.Visit>
{
    public VisitValidator(UserManager<User> userManager, ApiDbContext dbContext)
    {
        RuleFor(v => v.Date)
            .DateInFuture()
            .WithMessage("The date must be today or in the future.");

        RuleFor(v => (double) v.NumberOfGuests)
            .GreaterOrEqualToZero();

        RuleFor(v => (double) v.Tip!)
            .GreaterOrEqualToZero();

        RuleFor(v => v.Takeaway)
            .NotNull()
            .WithMessage("Takeaway field must be specified.");

        RuleFor(v => new Tuple<int, int>(v.RestaurantId, v.TableId))
            .TableExistsInRestaurant(dbContext)
            .WithMessage("The specified Table ID does not exist within the given Restaurant ID.");
        
    }
}