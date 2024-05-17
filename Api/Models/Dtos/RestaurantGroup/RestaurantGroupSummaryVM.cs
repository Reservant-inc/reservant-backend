using System.ComponentModel.DataAnnotations;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Reservant.Api.Models.Dtos.RestaurantGroup;

public class RestaurantGroupSummaryVM
{
    public required int RestaurantGroupId { get; init; }

    [Required, StringLength(50)]
    public required string Name { get; init; }

    public required int RestaurantCount { get; init; }
}
