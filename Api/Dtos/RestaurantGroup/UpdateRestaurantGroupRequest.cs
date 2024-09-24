using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Dtos.RestaurantGroup;

/// <summary>
/// Request to update a RestaurantGroup
/// </summary>
public class UpdateRestaurantGroupRequest
{
    /// <summary>
    /// New name
    /// </summary>
    public required string Name { get; init; }
}
