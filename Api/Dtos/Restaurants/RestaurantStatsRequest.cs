namespace Reservant.Api.Dtos.Restaurants;

/// <summary>
/// Restaurant statistics collection parameters
/// </summary>
public class RestaurantStatsRequest
{
    /// <summary>
    /// Default number of popular items to retrieve
    /// </summary>
    public const int DefaultPopularItemMaxCount = 10;

    /// <summary>
    /// Beginning date (inclusive) of the period over which to collect statistics
    /// </summary>
    public DateOnly? DateFrom { get; set; }

    /// <summary>
    /// Ending date (inclusive) of the period over which to collect statistics
    /// </summary>
    public DateOnly? DateUntil { get; set; }

    /// <summary>
    /// Number of popular items to retrieve
    /// </summary>
    public int? PopularItemMaxCount { get; set; } = DefaultPopularItemMaxCount;
}
