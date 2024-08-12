using Reservant.Api.Models.Dtos.Ingredient;

namespace Reservant.Api.Models.Dtos.Delivery;

public class DeliveryVM
{
    public required int Id { get; init; }
    public DateTime OrderTime { get; set; }
    public DateTime? DeliveredTime { get; set; }
    public int RestaurantId { get; set; }
    public string? UserId { get; set; } = null!;
    public required List<IngredientDeliveryVM> Ingredients { get; init; }
}
