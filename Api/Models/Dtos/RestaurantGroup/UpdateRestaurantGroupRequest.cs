using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.RestaurantGroup;

/// <summary>
/// Request to update a RestaurantGroup
/// </summary>
public class UpdateRestaurantGroupRequest
{
    /// <summary>
    /// New name
    /// </summary>
    [Required, StringLength(50)]
    public required string Name { get; init; }
}
