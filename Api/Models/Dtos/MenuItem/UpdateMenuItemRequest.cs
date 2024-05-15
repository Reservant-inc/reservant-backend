using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.MenuItem;

/// <summary>
/// Request to update a MenuItem
/// </summary>
public class UpdateMenuItemRequest
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
    /// Name in another language
    /// </summary>
    public string? AlternateName { get; set; }

    /// <summary>
    /// Zawartość alkoholu
    /// </summary>
    [Range(0, 100)]
    public decimal? AlcoholPercentage { get; set; }
}
