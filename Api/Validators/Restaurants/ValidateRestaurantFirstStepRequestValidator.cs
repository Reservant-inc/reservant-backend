using FluentValidation;
using Reservant.Api.Dtos.Restaurants;

namespace Reservant.Api.Validators.Restaurants;

/// <summary>
/// Validator for <see cref="ValidateRestaurantFirstStepRequest"/>
/// </summary>
public class ValidateRestaurantFirstStepRequestValidator : AbstractValidator<ValidateRestaurantFirstStepRequest>
{
    /// <inheritdoc />
    public ValidateRestaurantFirstStepRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Nip)
            .NotEmpty()
            .Nip();

        RuleFor(x => x.RestaurantType)
            .IsInEnum();

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(70)
            .IsValidAddress();

        RuleFor(x => x.PostalIndex)
            .NotEmpty()
            .PostalCode();

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(15)
            .IsValidCity();
    }
}
