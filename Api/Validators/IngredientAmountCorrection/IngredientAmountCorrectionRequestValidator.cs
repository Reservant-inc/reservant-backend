using FluentValidation;
using Reservant.Api.Dtos.Ingredient;

namespace Reservant.Api.Validators.IngredientAmountCorrection;

/// <summary>
/// Validator for IngredientAmountCorrectionRequest
/// </summary>
public class IngredientAmountCorrectionRequestValidator : AbstractValidator<IngredientAmountCorrectionRequest>
{
    /// <inheritdoc />
    public IngredientAmountCorrectionRequestValidator()
    {
        RuleFor(x => x.NewAmount)
            .GreaterOrEqualToZero();

        RuleFor(x => x.Comment)
            .NotEmpty()
            .MaximumLength(200);
    }
}
