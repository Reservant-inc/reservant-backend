using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.RestaurantGroup;

public class UpdateRestaurantGroupRequest
{
    [Required, StringLength(50)]
    public required string Name { get; init; }
}