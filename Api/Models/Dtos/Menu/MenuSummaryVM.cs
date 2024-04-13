using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.Menu;

/// <summary>
/// Basic info about a Menu
/// </summary>
public class MenuSummaryVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Required]
    public required int Id { get; set; }

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
}
