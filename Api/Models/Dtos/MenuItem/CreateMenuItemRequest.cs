using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.MenuItem;

/// <summary>
/// DTO containing info about a new menu item
/// </summary>
public class CreateMenuItemRequest
{
    /// <summary>
    /// Cena
    /// </summary>
    [Range(0, 500)]
    public decimal Price { get; set; }

    /// <summary>
    /// Nazwa
    /// </summary>
    [Required, StringLength(20)]
    public required string Name { get; set; }

    /// <summary>
    /// Zawartość alkoholu
    /// </summary>
    [Range(0, 100)]
    public decimal? AlcoholPercentage { get; set; }

    /// <summary>
    /// ID of the restaurant owning the menu
    /// </summary>
    public int RestaurantId { get; set; }
}
