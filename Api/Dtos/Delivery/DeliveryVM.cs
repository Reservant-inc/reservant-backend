using Reservant.Api.Dtos.Ingredient;

namespace Reservant.Api.Dtos.Delivery;

/// <summary>
/// Detailed info about a delivery
/// </summary>
public class DeliveryVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required int DeliveryId { get; init; }

    /// <summary>
    /// When was ordered
    /// </summary>
    public required DateTime OrderTime { get; set; }

    /// <summary>
    /// When was delivered
    /// </summary>
    public required DateTime? DeliveredTime { get; set; }

    /// <summary>
    /// ID of the restaurant
    /// </summary>
    public required int RestaurantId { get; set; }

    /// <summary>
    /// ID of the user who received the delivery
    /// </summary>
    public required Guid? UserId { get; set; } = null!;

    /// <summary>
    /// Ingredients delivered
    /// </summary>
    public required List<IngredientDeliveryVM> Ingredients { get; init; }
}
