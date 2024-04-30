using System.ComponentModel.DataAnnotations;
using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// Pozycja z menu
/// </summary>
public class MenuItem : ISoftDeletable
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Cena
    /// </summary>
    [Range(0, 500)]
    public required decimal Price { get; set; }

    /// <summary>
    /// Nazwa
    /// </summary>
    [Required, StringLength(20)]
    public required string Name { get; set; }

    /// <summary>
    /// Zawartość alkoholu
    /// </summary>
    [Range(0, 100)]
    public required decimal? AlcoholPercentage { get; set; }

    /// <summary>
    /// ID of the restaurant owning the menu
    /// </summary>
    public required int RestaurantId { get; set; }

    /// <summary>
    /// Menus that contain the item
    /// </summary>
    public ICollection<Menu>? Menus { get; set; }

    /// <summary>
    /// Navigation property for the restaurant owning the menu
    /// </summary>
    public Restaurant? Restaurant { get; set; }

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
