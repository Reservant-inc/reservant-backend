using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Reservant.Api.Dtos.Restaurants;
using Reservant.Api.Models;

namespace Reservant.Api.Dtos.RestaurantGroups;

/// <summary>
/// Information about a RestaurantGroup
/// </summary>
[AutoMap(typeof(RestaurantGroup))]
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
