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
    /// List of chosen max amount of menu items ordered that day ordered by count of orders per item
    /// </summary>
    public required Dictionary<string, int> PopularItems { get; init; }//map of names of dishes as well as their quantity sorted by popularity of menu item(number)(only as long as provided number)

    /// <summary>
    /// The dates used for reference for other list
    /// </summary>
    public required List<DateOnly> DateList { get; init; }

    /// <summary>
    /// Amount of customers served diuring days conresponding to the same indexes in Date list
    /// </summary>
    public required List<int> CustomerCountList { get; init; }

    /// <summary>
    ///  Amount of revenue days conresponding to the same indexes in Date list 
    /// </summary>
    public required List<decimal> RevenueList { get; init; }

}
