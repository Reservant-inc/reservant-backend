using System.ComponentModel.DataAnnotations;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Dtos.Menu;

/// <summary>
/// DTO containing info about a new menu
/// </summary>
public class CreateMenuRequest
{
    /// <summary>
    /// id of restaurant this menu will belog to
    /// </summary>
    public int RestaurantId { get; set; }

    /// <summary>
    /// Name of the menu
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Name of the menu in another language
    /// </summary>
    public string? AlternateName { get; set; }

    /// <summary>
    /// Typ menu
    /// </summary>
    public MenuType MenuType { get; set; }

    /// <summary>
    /// First day the menu is valid
    /// </summary>
    public DateOnly DateFrom { get; set; }

    /// <summary>
    /// Last day the menu is valid
    /// </summary>
    public DateOnly? DateUntil { get; set; }

    /// <summary>
    /// File name of the photo
    /// </summary>
    public required string Photo { get; set; }
}
