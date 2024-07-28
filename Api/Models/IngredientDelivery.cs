using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// Ingredient delivered within a Delivery
/// </summary>
public class IngredientDelivery : ISoftDeletable
{
    /// <summary>
    /// ID of the delivery
    /// </summary>
    public int DeliveryId { get; set; }

    /// <summary>
    /// ID of the ingredient
    /// </summary>
    public int IngredientId { get; set; }

    /// <summary>
    /// Amount ordered
    /// </summary>
    public double AmountOrdered { get; set; }

    /// <summary>
    /// Actual amount delivered
    /// </summary>
    public double AmountDelivered { get; set; }

    /// <summary>
    /// Expiry date of the ingredient
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Branded name of the ingredient as seen in the store
    /// </summary>
    public required string StoreName { get; set; }

    /// <summary>
    /// Navigation property for the delivery
    /// </summary>
    public Delivery Delivery { get; set; } = null!;

    /// <summary>
    /// Navigation property for the ingredient
    /// </summary>
    public Ingredient Ingredient { get; set; } = null!;

    /// <inheritdoc/>
    public bool IsDeleted { get; set; }
}
