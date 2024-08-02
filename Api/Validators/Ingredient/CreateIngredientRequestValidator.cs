using FluentValidation;
using Reservant.Api.Models.Dtos;

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
            .GreaterThanOrEqualTo(0)
            .WithMessage("MinimalAmount must be greater than or equal to 0.");

        RuleFor(i => i.AmountToOrder)
            .GreaterThanOrEqualTo(0)
            .When(i => i.AmountToOrder.HasValue)
            .WithMessage("AmountToOrder must be greater than or equal to 0.");
    }
}