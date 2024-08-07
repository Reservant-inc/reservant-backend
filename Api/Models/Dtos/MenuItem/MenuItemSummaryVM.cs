using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.MenuItem;

/// <summary>
/// Basic info about a MenuItem
/// </summary>
public class MenuItemSummaryVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Required]
    public required int MenuItemId { get; set; }

    /// <summary>
    /// Cena
    /// </summary>
    [Required, Range(0, 500)]
    public required decimal Price { get; set; }

    /// <summary>
    /// Nazwa
    /// </summary>
    [Required, StringLength(20)]
    public required string Name { get; set; }

    /// <summary>
    /// Name in another language
    /// </summary>
    public required string? AlternateName { get; set; }

    /// <summary>
    /// Zawartość alkoholu
    /// </summary>
    [Range(0, 100)]
    public required decimal? AlcoholPercentage { get; set; }

    /// <summary>
    /// File name of the photo
    /// </summary>
    [Required, StringLength(50)]
    public required string Photo { get; set; }
}
