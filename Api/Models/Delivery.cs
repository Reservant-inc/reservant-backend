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
    public int Id { get; set; }

    /// <summary>
    /// When was ordered
    /// </summary>
    public DateTime OrderTime { get; set; }

    /// <summary>
    /// When was delivered
    /// </summary>
    public DateTime DeliveredTime { get; set; }

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
    public string UserId { get; set; } = null!;

    /// <summary>
    /// Navigation property for the user who received the delivery
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Ingredients delivered
    /// </summary>
    public ICollection<IngredientDelivery> Ingredients { get; set; } = null!;

    /// <inheritdoc/>
    public bool IsDeleted { get; set; }
}
