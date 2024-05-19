using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Visit;

namespace Reservant.Api.Validators.Visit;

/// <summary>
/// Validator for CreateVisitRequest
/// </summary>
public class CreateVisitRequestValidator : AbstractValidator<CreateVisitRequest>
{
    /// <inheritdoc />
    public CreateVisitRequestValidator(UserManager<User> userManager, ApiDbContext dbContext)
    {
        RuleFor(v => v.Date)
            .DateInFuture()
            .WithMessage("The date must be today or in the future.");

        RuleFor(v => v.NumberOfGuests)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Number of guests must be a non-negative value.")
            .WithErrorCode(ErrorCodes.NumberOfGuests);

        RuleFor(v => v.Tip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Tip must be a non-negative value.")
            .WithErrorCode(ErrorCodes.Tip);

        RuleFor(v => v.Takeaway)
            .NotNull()
            .WithMessage("Takeaway field must be specified.");

        RuleFor(v => v.RestaurantId)
            .RestaurantExists(dbContext);

        RuleFor(v => new Tuple<int, int>(v.RestaurantId, v.TableId))
            .TableExistsInRestaurant(dbContext)
            .WithMessage("The specified Table ID does not exist within the given Restaurant ID.");
        
        RuleForEach(v => v.Participants)
            .UserExists(userManager)
            .WithMessage("User with ID {PropertyValue} does not exist.");
    }
    
}