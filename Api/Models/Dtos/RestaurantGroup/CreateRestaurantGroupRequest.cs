using System.ComponentModel.DataAnnotations;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Models.Dtos.RestaurantGroup;

public class CreateRestaurantGroupRequest
{
    [Required, StringLength(50)]
    public required string Name { get; init; }

    [Required, Length(1, 10)]
    public required List<int> RestaurantIds { get; init; }
}
