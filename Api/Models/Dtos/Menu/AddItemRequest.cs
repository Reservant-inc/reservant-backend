using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.Menu;

/// <summary>
/// Request to add an existing item to a menu
/// </summary>
public class AddItemsRequest
{
    /// <summary>
    /// IDs of the MenuItem's to add
    /// </summary>
    [Required]
    public required List<int> ItemIds { get; init; }
}
