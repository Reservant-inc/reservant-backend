using Reservant.Api.Dtos.Ingredients;

namespace Reservant.Api.Dtos.Deliveries;

/// <summary>
/// Request to create a Delivery
/// </summary>
public class CreateDeliveryRequest
{
    /// <summary>
    /// ID of the restaurant
    /// </summary>
    public required int RestaurantId { get; set; }

    /// <summary>
    /// Ingredients ordered
    /// </summary>
    public required List<IngredientDeliveryVM> Ingredients { get; init; }
}
