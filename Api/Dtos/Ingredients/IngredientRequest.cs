namespace Reservant.Api.Dtos.Ingredients;

/// <summary>
/// Used in DTOs to specify menu item ingredients
/// </summary>
public class IngredientRequest
{
    /// <summary>
    /// ID of the ingredient
    /// </summary>
    public required int IngredientId { get; set; }

    /// <summary>
    /// Amount of the ingredient used
    /// </summary>
    public required double AmountUsed { get; set; }
}
