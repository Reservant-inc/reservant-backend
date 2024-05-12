using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Models;

namespace Reservant.Api.Validators.Visit;

public class VisitValidator: AbstractValidator<Models.Visit>
{
    public VisitValidator(UserManager<User> userManager)
    {
        RuleFor(v => v.Date)
            .Must(date => date >= DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("The date must be today or in the future.");

        RuleFor(v => v.NumberOfGuests)
            .GreaterThanOrEqualTo(1)
            .WithMessage("There must be at least one guest.");

        RuleFor(v => v.Tip)
            .Must(tip => tip is null or >= 0)
            .WithMessage("Tip must be a non-negative value.");

        RuleFor(v => v.Takeaway)
            .NotNull()
            .WithMessage("Takeaway field must be specified.");

        RuleFor(v => v.RestaurantId)
            .GreaterThan(0)
            .WithMessage("Invalid Restaurant ID.");

        RuleFor(v => v.TableId)
            .GreaterThan(0)
            .WithMessage("Invalid Table ID.");

        RuleForEach(v => v.Participants)
            .NotEmpty()
            .WithMessage("Participant names must not be empty.");
        
    }
}