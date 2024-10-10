using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Dtos.Menus;

/// <summary>
/// Request to add existing items to a menu
/// </summary>
public class AddItemsRequest
{
    /// <summary>
    /// IDs of the MenuItem's to add
    /// </summary>
    public required List<int> ItemIds { get; init; }
}
