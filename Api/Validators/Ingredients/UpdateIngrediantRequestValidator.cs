using FluentValidation;
using Reservant.Api.Dtos.Ingredients;

namespace Reservant.Api.Validators.Ingredients;

/// <summary>
/// Validator for UpdateIngredientRequest
/// </summary>
public class UpdateIngrediantRequestValidator : AbstractValidator<UpdateIngredientRequest>
{
    /// <inheritdoc/>
    public UpdateIngrediantRequestValidator()
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