using FluentValidation;
using Reservant.Api.Dtos.Ingredient;

namespace Reservant.Api.Validators.Ingredient;

/// <summary>
/// Validator for CreateIngredientRequest
/// </summary>
public class CreateIngredientRequestValidator : AbstractValidator<CreateIngredientRequest>
{
    /// <inheritdoc/>
    public CreateIngredientRequestValidator()
    {
        RuleFor(i => i.PublicName)
            .NotEmpty()
            .WithMessage("PublicName cannot be empty.")
            .MaximumLength(20);

        RuleFor(i => i.UnitOfMeasurement)
            .IsInEnum()
            .WithMessage("UnitOfMeasurement must be a valid value.");

        RuleFor(i => i.MinimalAmount)
            .GreaterOrEqualToZero();

        RuleFor(i => i.AmountToOrder)
            .GreaterOrEqualToZero();
    }
}