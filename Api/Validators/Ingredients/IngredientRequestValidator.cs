using FluentValidation;
using Reservant.Api.Dtos.Ingredients;

namespace Reservant.Api.Validators.Ingredients;

/// <summary>
/// Validator for Ingredient request
/// </summary>
public class IngredientRequestValidator : AbstractValidator<IngredientRequest>
{
    /// <inheritdoc/>
    public IngredientRequestValidator()
    {
        RuleFor(x => x.AmountUsed)
            .GreaterOrEqualToOne();
    }
}
