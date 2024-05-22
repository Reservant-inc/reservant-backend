using System.ComponentModel.DataAnnotations;
using Reservant.Api.Models.Dtos.MenuItem;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Models.Dtos.Menu;

/// <summary>
/// Info about a Menu
/// </summary>
public class MenuVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Required]
    public required int Id { get; set; }

    /// <summary>
    /// Name of the menu
    /// </summary>
    [Required]
    public required string Name { get; set; }

    /// <summary>
    /// Name of the menu in another language
    /// </summary>
    public required string? AlternateName { get; set; }

    /// <summary>
    /// Typ menu
    /// </summary>
    [Required]
    public required MenuType MenuType { get; set; }

    /// <summary>
    /// First day the menu is valid
    /// </summary>
    [Required]
    public required DateOnly DateFrom { get; set; }

    /// <summary>
    /// Last day the menu is valid
    /// </summary>
    public required DateOnly? DateUntil { get; set; }

    /// <summary>
    /// Menu items
    /// </summary>
    [Required]
    public required List<MenuItemSummaryVM> MenuItems { get; set; }
}
