using FluentValidation;
using Reservant.Api.Dtos.Ingredient;

namespace Reservant.Api.Validators.Ingredient;

/// <summary>
/// Validator for Ingredient request
/// </summary>
public class IngredientRequestValidator : AbstractValidator<IngredientRequest>
{
    /// <inheritdoc/>
    public IngredientRequestValidator()
    {
        RuleFor(x => x.AmountUsed)
            .GreaterOrEqualToZero();
    }
}
