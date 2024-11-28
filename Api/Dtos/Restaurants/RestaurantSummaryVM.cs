using System.Text.Json.Serialization;
using Reservant.Api.Models.Enums;
using Reservant.Api.Dtos.Location;

namespace Reservant.Api.Dtos.Restaurants;

/// <summary>
/// Basic restaurant info
/// </summary>
public class RestaurantSummaryVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required int RestaurantId { get; init; }

    /// <summary>
    /// Nazwa
    /// </summary>
    /// <example>McJohn's</example>
    public required string Name { get; init; }

    /// <summary>
    /// Type of the establishment
    /// </summary>
    public required RestaurantType RestaurantType { get; set; }

    /// <summary>
    /// Adres
    /// </summary>
    ///  <example>ul. Koszykowa 86</example>
    public required string Address { get; init; }

    /// <summary>
    /// City of the restaurant
    /// </summary>
    /// <example>Warszawa</example>
    public required string City { get; init; }

    /// <summary>
    /// Geolocation class having Longitude, Latitude
    /// </summary>
    /// <example></example>
    public required Geolocation Location { get; init; }

    /// <summary>
    /// URI of the logo
    /// </summary>
    public required string Logo { get; init; }

    /// <summary>
    /// Optional description
    /// </summary>
    public required string? Description { get; init; }

    /// <summary>
    /// Deposit
    /// </summary>
    public required decimal? ReservationDeposit { get; init; }

    /// <summary>
    /// Max reservation duration
    /// </summary>
    public required int MaxReservationDurationMinutes { get; set; }

    /// <summary>
    /// Whether we provide delivery for the restaurant
    /// </summary>
    public required bool ProvideDelivery { get; init; }

    /// <summary>
    /// Restaurant tags
    /// </summary>
    public required List<string> Tags { get; init; }

    /// <summary>
    /// Rating of the restaurant based on the reviews
    /// </summary>
    public required double Rating { get; set; }

    /// <summary>
    /// Number of reviews about this restaurant
    /// </summary>
    public required int NumberReviews { get; set; }

    /// <summary>
    /// Hours when the restaurant is open
    /// </summary>
    public required List<AvailableHoursVM> OpeningHours { get; set; }

    /// <summary>
    /// Whether the restaurant is archived
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public required bool IsArchived { get; set; }
}
