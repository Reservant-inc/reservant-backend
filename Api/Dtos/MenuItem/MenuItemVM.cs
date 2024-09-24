using Reservant.Api.Dtos.Ingredient;
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Dtos.MenuItem;

/// <summary>
/// Info about a MenuItem
/// </summary>
public class MenuItemVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required int MenuItemId { get; set; }

    /// <summary>
    /// Cena
    /// </summary>
    public required decimal Price { get; set; }

    /// <summary>
    /// Nazwa
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Name in another language
    /// </summary>
    public required string? AlternateName { get; set; }

    /// <summary>
    /// Zawartość alkoholu
    /// </summary>
    public required decimal? AlcoholPercentage { get; set; }

    /// <summary>
    /// File name of the photo
    /// </summary>
    public required string Photo { get; set; }

    /// <summary>
    /// Ingredients used in the menu item
    /// </summary>
    public required List<MenuItemIngredientVM> Ingredients { get; set; }
}
