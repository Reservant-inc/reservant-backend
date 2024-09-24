using System.ComponentModel.DataAnnotations;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Dtos.RestaurantGroup;

public class CreateRestaurantGroupRequest
{
    public required string Name { get; init; }

    public required List<int> RestaurantIds { get; init; }
}
