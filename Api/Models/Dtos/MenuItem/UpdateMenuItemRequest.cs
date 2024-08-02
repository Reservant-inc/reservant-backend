namespace Reservant.Api.Models.Dtos.MenuItem;

/// <summary>
/// Request to update a MenuItem
/// </summary>
public class UpdateMenuItemRequest
{
    /// <summary>
    /// Cena
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Nazwa
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Name in another language
    /// </summary>
    public string? AlternateName { get; set; }

    /// <summary>
    /// Zawartość alkoholu
    /// </summary>
    public decimal? AlcoholPercentage { get; set; }

    /// <summary>
    /// File name of the photo
    /// </summary>
    public required string Photo { get; set; }

    /// <summary>
    /// Ingredients required to make the item
    /// </summary>
    public required List<IngredientRequest> Ingredients { get; set; }
}
