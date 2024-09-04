using Reservant.Api.Models.Dtos.Ingredient;

namespace Reservant.Api.Dtos.Delivery;

public class DeliveryVM
{
    public required int DeliveryId { get; init; }
    public required DateTime OrderTime { get; set; }
    public required DateTime? DeliveredTime { get; set; }
    public required int RestaurantId { get; set; }
    public required string? UserId { get; set; } = null!;
    public required List<IngredientDeliveryVM> Ingredients { get; init; }
}
