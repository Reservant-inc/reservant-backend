using Reservant.Api.Models.Dtos.Ingredient;

namespace Reservant.Api.Models.Dtos.Delivery;

public class CreateDeliveryRequest
{
    public required List<IngredientDeliveryVM> Positions { get; init; }
}
