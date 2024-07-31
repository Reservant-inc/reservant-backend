using FluentValidation;
using Reservant.Api.Models;

public class IngredientValidator : AbstractValidator<Ingredient>
{
    public IngredientValidator()
    {
        RuleFor(x => x.PublicName).NotEmpty().WithMessage("PublicName cannot be empty.");
        RuleFor(x => x.UnitOfMeasurement).IsInEnum().WithMessage("UnitOfMeasurement must be a valid value.");
        RuleFor(x => x.MinimalAmount).GreaterThanOrEqualTo(0).WithMessage("MinimalAmount must be greater than or equal to 0.");
        RuleFor(x => x.AmountToOrder).GreaterThanOrEqualTo(0).When(x => x.AmountToOrder.HasValue).WithMessage("AmountToOrder must be greater than or equal to 0.");
    }
}
