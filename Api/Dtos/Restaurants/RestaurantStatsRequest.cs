namespace Reservant.Api.Dtos.Restaurants;

/// <summary>
/// Different criteria to search restaurants for
/// </summary>
public class RestaurantStatsRequest
{
    /// <summary>
    /// Function for retrivewing restaurant statistics from given time period
    /// </summary>
    public const int DefaultPopularItemMaxCount = 10;

    /// <summary>
    /// starting date for downloading statistic(included)
    /// </summary>
    public DateOnly? DateFrom { get; set; }

    /// <summary>
    /// ending date for downloading statsitcs(excuded)
    /// </summary>
    public DateOnly? DateUntil { get; set; }

    /// <summary>
    /// maximal amount of items that can be put on popular list
    /// </summary>
    public int? PopularItemMaxCount { get; set; } = DefaultPopularItemMaxCount;
}
