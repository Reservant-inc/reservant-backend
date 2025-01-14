using FluentValidation;
using Reservant.Api.Models;

namespace Reservant.Api.Validators.Restaurants;

/// <summary>
/// Validator for Restaurant
/// </summary>
public class RestaurantValidator : AbstractValidator<Models.Restaurant>
{
    /// <inheritdoc />
    public RestaurantValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(r => r.Nip)
            .NotNull()
            .Nip();

        RuleFor(r => r.Address)
            .NotEmpty()
            .MaximumLength(70);

        RuleFor(r => r.MaxReservationDurationMinutes)
            .GreaterThanOrEqualTo(Visit.MinReservationDurationMinutes);

        RuleFor(r => r.PostalIndex)
            .NotNull()
            .PostalCode();

        RuleFor(r => r.City)
            .NotEmpty()
            .MaximumLength(15)
            .IsValidCity();

        RuleFor(r => r.Description)
            .Length(1, 200);

        RuleFor(r => r.Location)
            .IsValidLocation();
    }
}
