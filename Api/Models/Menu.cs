using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models;

/// <summary>
/// Menu
/// </summary>
public class Menu
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    public int Id { get; set; }

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
    public required int RestaurantId { get; set; }

    /// <summary>
    /// Navigation collection for the items
    /// </summary>
    public ICollection<MenuItem>? MenuItems { get; set; }

    /// <summary>
    /// Navigation property for the restaurant owning the menu
    /// </summary>
    public Restaurant? Restaurant { get; set; }
}
