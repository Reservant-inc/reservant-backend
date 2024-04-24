using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.Menu;

/// <summary>
/// Request to update a Menu
/// </summary>
public class UpdateMenuRequest
{
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
}
