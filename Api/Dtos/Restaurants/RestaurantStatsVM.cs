using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validation;
using Reservant.Api.Dtos.Location;
using Reservant.Api.Dtos.Tables;

namespace Reservant.Api.Dtos.Restaurants;

/// <summary>
/// Statistic of restaurants in a given day
/// </summary>
public class RestaurantStatsVM
{
    /// <summary>
    /// Id of a restaurnat
    /// </summary>
    public required int RestaurantId { get; init; }

     /// <summary>
    /// The date to which the statistics refer
    /// </summary>
    public required List<DayStatsVM> RestaurantStat { get; init; }

}
