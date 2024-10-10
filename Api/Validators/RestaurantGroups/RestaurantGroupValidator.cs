using FluentValidation;

namespace Reservant.Api.Validators.RestaurantGroups;

/// <summary>
/// Validator for <see cref="Models.RestaurantGroup"/>
/// </summary>
public class RestaurantGroupValidator : AbstractValidator<Models.RestaurantGroup>
{
    /// <inheritdoc />
    public RestaurantGroupValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);
    }
}
