using Reservant.Api.Models.Enums;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Ingredients;

/// <summary>
/// Request to create a new ingredient
/// </summary>
public class CreateIngredientRequest
{
    /// <summary>
    /// Name of the ingredient item as shown to the customer
    /// </summary>
    public required string PublicName { get; set; }

    /// <summary>
    /// Unit of measurement used for amount
    /// </summary>
    public required UnitOfMeasurement UnitOfMeasurement { get; set; }

    /// <summary>
    /// Minimal amount considered enough
    /// </summary>
    public required double MinimalAmount { get; set; }

    /// <summary>
    /// When added to the shopping list, the amount to order
    /// </summary>
    public double? AmountToOrder { get; set; }

    /// <summary>
    /// Starting amount of the ingredient
    /// </summary>
    public required double Amount { get; set; }

    ///NEWLY ADDED
    /// <summary>
    /// Id of restaurant using this ingrediants
    /// </summary>
    public required int RestaurantId { get; set; }
}
