namespace Reservant.Api.Dtos.Restaurants;

/// <summary>
/// Different criteria to search restaurants for
/// </summary>
public class RestaurantSearchParams
{
    /// <summary>
    /// Latitude of the point to search from; if provided the restaurants will be sorted by distance
    /// </summary>
    public double? OrigLat { get; set; }

    /// <summary>
    /// Longitude of the point to search from; if provided the restaurants will be sorted by distance
    /// </summary>
    public double? OrigLon { get; set; }

    /// <summary>
    /// Search by name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Maximum number of tags allowed
    /// </summary>
    public const int MaxNumberOfTags = 4;

    /// <summary>
    /// Search restaurants that have certain tags (up to 4)
    /// </summary>
    public HashSet<string> Tags { get; set; } = [];

    /// <summary>
    /// Minimum value for MinRating
    /// </summary>
    public const int MinRatingMin = 0;

    /// <summary>
    /// Maximum value for MinRating
    /// </summary>
    public const int MinRatingMax = 5;

    /// <summary>
    /// Search restaurants with at least this many stars
    /// </summary>
    public int? MinRating { get; set; }

    /// <summary>
    /// Search within a rectangular area: first point's latitude
    /// </summary>
    public double? Lat1 { get; set; }

    /// <summary>
    /// Search within a rectangular area: first point's longitude
    /// </summary>
    public double? Lon1 { get; set; }

    /// <summary>
    /// Search within a rectangular area: second point's latitude
    /// </summary>
    public double? Lat2 { get; set; }

    /// <summary>
    /// Search within a rectangular area: second point's longitude
    /// </summary>
    public double? Lon2 { get; set; }

    /// <summary>
    /// Page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Items per page
    /// </summary>
    public int PerPage { get; set; }
}
