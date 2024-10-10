using FluentValidation;
using Reservant.Api.Dtos.Ingredients;

namespace Reservant.Api.Validators.IngredientAmountCorrections;

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
