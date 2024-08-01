using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// Ingredient used to cook a menu item
/// </summary>
public class IngredientMenuItem : ISoftDeletable
{
    /// <summary>
    /// ID of the menu item
    /// </summary>
    public int MenuItemId { get; set; }

    /// <summary>
    /// ID of the ingredient
    /// </summary>
    public int IngredientId { get; set; }

    /// <summary>
    /// Amount of the ingredient used
    /// </summary>
    public double AmountUsed {  get; set; }

    /// <summary>
    /// Navigation property for the menu item
    /// </summary>
    public MenuItem MenuItem { get; set; } = null!;

    /// <summary>
    /// Navigation property for the 
    /// </summary>
    public Ingredient Ingredient { get; set; } = null!;

    /// <inheritdoc/>
    public bool IsDeleted { get; set; }
}
