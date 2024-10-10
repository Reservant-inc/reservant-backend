namespace Reservant.Api.Dtos.Ingredients;

/// <summary>
/// Information about a MenuItems' ingredient
/// </summary>
public class MenuItemIngredientVM
{
    /// <summary>
    /// ID of the ingredient
    /// </summary>
    public required int IngredientId { get; set; }

    /// <summary>
    /// Name of the ingredient item as shown to the customer
    /// </summary>
    public required string PublicName { get; set; }

    /// <summary>
    /// Amount of the ingredient used
    /// </summary>
    public required double AmountUsed { get; set; }
}
