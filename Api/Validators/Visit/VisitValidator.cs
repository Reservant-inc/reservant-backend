using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Data;

namespace Reservant.Api.Validators.Visit;

/// <summary>
/// Validator for Visit
/// </summary>
public class VisitValidator : AbstractValidator<Models.Visit>
{
    /// <inheritdoc />
    public VisitValidator(UserManager<Models.User> userManager, ApiDbContext dbContext)
    {
        RuleFor(v => v.Date)
            .DateTimeInFuture();

        RuleFor(v => (double) v.NumberOfGuests)
            .GreaterOrEqualToZero();

        RuleFor(v => (double) v.Tip!)
            .GreaterOrEqualToZero();

        RuleFor(v => v.Takeaway)
            .NotNull()
            .WithMessage("Takeaway field must be specified.");

        RuleFor(v => new Tuple<int, int>(v.RestaurantId, v.TableId))
            .TableExistsInRestaurant(dbContext);

    }
}
