using System.ComponentModel.DataAnnotations;
using Reservant.Api.Models.Dtos.Restaurant;

namespace Reservant.Api.Models.Dtos;

public class RestaurantGroupVM
{
    public required int Id { get; init; }

    [Required, StringLength(50)]
    public required string Name { get; init; }

    [Required, Length(1, 10)]
    public required List<RestaurantSummaryVM> Restaurants { get; init; }
}
