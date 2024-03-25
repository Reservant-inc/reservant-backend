using System.ComponentModel.DataAnnotations;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Reservant.Api.Models.Vmodels;

public class RestaurantGroupSummaryVM
{
    public required int Id { get; init; }

    [Required, StringLength(50)]
    public required string Name { get; init; }

    public required int RestaurantCount { get; init; }
}
