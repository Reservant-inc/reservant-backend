using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.Menu;

/// <summary>
/// DTO containing info about a new menu
/// </summary>
public class CreateMenuRequest
{
    /// <summary>
    /// id of restaurant this menu will belog to
    /// </summary>
    [Required]
    public int restaurantId { get; set; }

    /// <summary>
    /// Typ menu
    /// </summary>
    [Required]
    public MenuType MenuType { get; set; }

    /// <summary>
    /// First day the menu is valid
    /// </summary>
    [Required]
    public DateOnly DateFrom { get; set; }

    /// <summary>
    /// Last day the menu is valid
    /// </summary>
    public DateOnly? DateUntil { get; set; }

    /// <summary>
    /// ID of the restaurant owning the menu
    /// </summary>
    // [Required]
    // public int RestaurantId { get; set; }
}
