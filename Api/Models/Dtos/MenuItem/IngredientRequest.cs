namespace Reservant.Api.Models.Dtos.MenuItem;

/// <summary>
/// Used in DTOs to specify menu item ingredients
/// </summary>
public class IngredientRequest
{
    /// <summary>
    /// ID of the ingredient
    /// </summary>
    public int IngredientId { get; set; }

    /// <summary>
    /// Amount of the ingredient used
    /// </summary>
    public double AmountUsed { get; set; }
}
