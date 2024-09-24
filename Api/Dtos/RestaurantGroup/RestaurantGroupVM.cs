using System.ComponentModel.DataAnnotations;
using Reservant.Api.Dtos.Restaurant;

namespace Reservant.Api.Dtos.RestaurantGroup;

/// <summary>
/// Information about a RestaurantGroup
/// </summary>
public class RestaurantGroupVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required int RestaurantGroupId { get; init; }

    /// <summary>
    /// Name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Information about its restaurants
    /// </summary>
    public required List<RestaurantSummaryVM> Restaurants { get; init; }
}
