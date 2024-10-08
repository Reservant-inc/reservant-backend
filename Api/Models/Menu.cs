using System.ComponentModel.DataAnnotations;
using Reservant.Api.Data;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Models;

/// <summary>
/// Menu
/// </summary>
public class Menu : ISoftDeletable
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    public int MenuId { get; set; }

    /// <summary>
    /// Name of the menu
    /// </summary>
    [StringLength(20)]
    public required string Name { get; set; }

    /// <summary>
    /// Name in another language
    /// </summary>
    [StringLength(20)]
    public string? AlternateName { get; set; }

    /// <summary>
    /// Typ menu
    /// </summary>
    public required MenuType MenuType { get; set; }

    /// <summary>
    /// First day the menu is valid
    /// </summary>
    public required DateOnly DateFrom { get; set; }

    /// <summary>
    /// Last day the menu is valid
    /// </summary>
    public required DateOnly? DateUntil { get; set; }

    /// <summary>
    /// ID of the restaurant owning the menu
    /// </summary>
    public int RestaurantId { get; set; }

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
    /// Navigation collection for the items
    /// </summary>
    public ICollection<MenuItem> MenuItems { get; set; } = null!;

    /// <summary>
    /// Navigation property for the restaurant owning the menu
    /// </summary>
    public Restaurant Restaurant { get; set; } = null!;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
