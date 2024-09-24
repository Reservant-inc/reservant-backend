using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    [Column(TypeName = "decimal(5, 2)")]
    public required decimal Price { get; set; }

    /// <summary>
    /// Nazwa
    /// </summary>
    [StringLength(20)]
    public required string Name { get; set; }

    /// <summary>
    /// Name in another language
    /// </summary>
    [StringLength(20)]
    public string? AlternateName { get; set; }

    /// <summary>
    /// Zawartość alkoholu
    /// </summary>
    [Column(TypeName = "decimal(4, 1)")]
    public required decimal? AlcoholPercentage { get; set; }

    /// <summary>
    /// ID of the restaurant owning the menu
    /// </summary>
    public int RestaurantId { get; set; }

    /// <summary>
    /// Menus that contain the item
    /// </summary>
    public ICollection<Menu> Menus { get; set; } = null!;

    /// <summary>
    /// Navigation property for the restaurant owning the menu
    /// </summary>
    public Restaurant Restaurant { get; set; } = null!;

    /// <summary>
    /// File name of the photo
    /// </summary>
    [StringLength(50)]
    public required string PhotoFileName { get; set; }

    /// <summary>
    /// Navigation property for the photo upload
    /// </summary>
    public FileUpload Photo { get; set; } = null!;

    /// <summary>
    /// Navigation property for the ingredients
    /// </summary>
    public ICollection<IngredientMenuItem> Ingredients { get; set; } = null!;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
