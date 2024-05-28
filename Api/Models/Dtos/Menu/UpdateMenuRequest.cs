using System.ComponentModel.DataAnnotations;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Models.Dtos.Menu;

/// <summary>
/// Request to update a Menu
/// </summary>
public class UpdateMenuRequest
{
    
    /// <summary>
    /// Name of the menu
    /// </summary>
    [Required]
    public required string Name { get; set; }

    /// <summary>
    /// Name of the menu in another language
    /// </summary>
    public string? AlternateName { get; set; }

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
