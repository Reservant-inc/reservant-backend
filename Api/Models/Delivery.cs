using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// Ingredient delivery
/// </summary>
public class Delivery : ISoftDeletable
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public int DeliveryId { get; set; }

    /// <summary>
    /// When was ordered
    /// </summary>
    public DateTime OrderTime { get; set; }

    /// <summary>
    /// When was delivered
    /// </summary>
    public DateTime? DeliveredTime { get; set; }

    /// <summary>
    /// When the delivery was canceled
    /// </summary>
    public DateTime? CanceledTime { get; set; }

    /// <summary>
    /// ID of the restaurant
    /// </summary>
    public int RestaurantId { get; set; }

    /// <summary>
    /// Navigation property for the restaurant
    /// </summary>
    public Restaurant Restaurant { get; set; } = null!;

    /// <summary>
    /// ID of the user who received the delivery
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Navigation property for the user who received the delivery
    /// </summary>
    public User? User { get; set; } = null!;

    /// <summary>
    /// Ingredients delivered
    /// </summary>
    public ICollection<IngredientDelivery> Ingredients { get; set; } = null!;

    /// <inheritdoc/>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Whether the delivery is still not confirmed nor canceled
    /// </summary>
    public bool IsPending => DeliveredTime is null && CanceledTime is null;
}
