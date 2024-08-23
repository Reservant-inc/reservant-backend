using Reservant.Api.Models.Enums;

namespace Reservant.Api.Dtos.Ingredient;

/// <summary>
/// Request to create a new ingredient
/// </summary>
public class UpdateIngredientRequest
{
    /// <summary>
    /// Name of the ingredient item as shown to the customer
    /// </summary>
    public required string PublicName { get; set; }

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

    /// <summary>
    /// Starting amount of the ingredient
    /// </summary>
    public required double Amount { get; set; }

    /// <summary>
    /// Property that tells what menu items used what amount of ingredients
    /// </summary>
    public required UsedInMenuItemDto MenuItem { get; set; }
}
