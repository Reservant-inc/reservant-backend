using System.ComponentModel.DataAnnotations;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Dtos.Menus;

/// <summary>
/// DTO containing info about a new menu
/// </summary>
public class CreateMenuRequest
{
    /// <summary>
    /// id of restaurant this menu will belog to
    /// </summary>
    public required int RestaurantId { get; set; }

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
    public required MenuType MenuType { get; set; }

    /// <summary>
    /// First day the menu is valid
    /// </summary>
    public required DateOnly DateFrom { get; set; }

    /// <summary>
    /// Last day the menu is valid
    /// </summary>
    public required DateOnly? DateUntil { get; set; }
}
