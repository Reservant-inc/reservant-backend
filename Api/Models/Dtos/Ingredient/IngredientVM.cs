using Reservant.Api.Models.Enums;

namespace Reservant.Api.Models.Dtos.Ingredient;

/// <summary>
/// ViewModel for Ingredient
/// </summary>
public class IngredientVM
{
    /// <summary>
    /// Unique ID of the ingredient
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the ingredient item as shown to the customer
    /// </summary>
    public string PublicName { get; set; } = string.Empty;

    /// <summary>
    /// Unit of measurement used for amount
    /// </summary>
    public UnitOfMeasurement UnitOfMeasurement { get; set; }

    /// <summary>
    /// Minimal amount considered enough
    /// </summary>
    public double MinimalAmount { get; set; }

    /// <summary>
    /// When added to the shopping list, the amount to order
    /// </summary>
    public double? AmountToOrder { get; set; }

    public double AmountUsed { get; set; }
}