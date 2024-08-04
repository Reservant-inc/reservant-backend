using FluentValidation;

namespace Reservant.Api.Validators.Ingredient;


/// <summary>
/// Validator for Ingredient
/// </summary>
public class IngredientValidator : AbstractValidator<Models.Ingredient>
{
    /// <inheritdoc/>
    public IngredientValidator()
    {
        RuleFor(x => x.PublicName)
            .NotEmpty()
            .WithMessage("PublicName cannot be empty.");

        RuleFor(x => x.UnitOfMeasurement)
            .IsInEnum()
            .WithMessage("UnitOfMeasurement must be a valid value.");

        RuleFor(x => x.MinimalAmount)
            .GreaterOrEqualToZero();

        RuleFor(x => x.AmountToOrder)
            .GreaterOrEqualToZero();
    }
}