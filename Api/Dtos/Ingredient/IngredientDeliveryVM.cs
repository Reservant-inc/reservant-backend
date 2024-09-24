namespace Reservant.Api.Dtos.Ingredient;

/// <summary>
/// Information about an ingredient in the context of a delivery
/// </summary>
public class IngredientDeliveryVM
{
    /// <summary>
    /// ID of the delivery
    /// </summary>
    public required int DeliveryId { get; set; }

    /// <summary>
    /// ID of the ingredient
    /// </summary>
    public required int IngredientId { get; set; }

    /// <summary>
    /// Amount of the ingredient ordered
    /// </summary>
    public required double AmountOrdered { get; set; }

    /// <summary>
    /// Amount of the ingredient delivered
    /// </summary>
    public required double? AmountDelivered { get; set; }

    /// <summary>
    /// Expiry date of the ingredient
    /// </summary>
    public required DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Name of the actual product ordered
    /// </summary>
    public required string StoreName { get; set; }
}
