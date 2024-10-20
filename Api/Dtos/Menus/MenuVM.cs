using Reservant.Api.Dtos.MenuItems;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Dtos.Menus;

/// <summary>
/// Info about a Menu
/// </summary>
public class MenuVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required int MenuId { get; set; }

    /// <summary>
    /// Name of the menu
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Name of the menu in another language
    /// </summary>
    public required string? AlternateName { get; set; }

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
    /// Menu items
    /// </summary>
    public required List<MenuItemSummaryVM> MenuItems { get; set; }
}
