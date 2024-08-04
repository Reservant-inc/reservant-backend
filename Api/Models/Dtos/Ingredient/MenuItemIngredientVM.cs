namespace Reservant.Api.Models.Dtos.Ingredient;

/// <summary>
/// Information about a MenuItems' ingredient
/// </summary>
public class MenuItemIngredientVM
{
    /// <summary>
    /// ID of the ingredient
    /// </summary>
    public int IngredientId { get; set; }

    /// <summary>
    /// Name of the ingredient item as shown to the customer
    /// </summary>
    public required string PublicName { get; set; }

    /// <summary>
    /// Amount of the ingredient used
    /// </summary>
    public double AmountUsed { get; set; }
}
