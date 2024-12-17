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
public class dayStatsVM
{
    /// <summary>
    /// The date to which the statistics refer
    /// </summary>
    public required DateOnly StatsReferenceDate { get; init; }

    /// <summary>
    /// Amount of customers served diuring the day
    /// </summary>
    public required int CustomerCount { get; init; }

    /// <summary>
    /// Sum of all sales 
    /// </summary>
    public required decimal Revenue { get; init; }

    /// <summary>
    /// List of chosen max amount of menu items ordered that day ordered by count of orders per item
    /// </summary>
    public required string PopularItems { get; init; }
}
