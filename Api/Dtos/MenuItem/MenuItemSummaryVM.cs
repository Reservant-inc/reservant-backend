using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Dtos.MenuItem;

/// <summary>
/// Basic info about a MenuItem
/// </summary>
public class MenuItemSummaryVM
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
}
