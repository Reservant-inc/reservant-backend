using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Dtos.Menu;

/// <summary>
/// Request to remove items to a menu
/// </summary>
public class RemoveItemsRequest
{
    /// <summary>
    /// IDs of the MenuItem's to add
    /// </summary>
    public required List<int> ItemIds { get; init; }
}
