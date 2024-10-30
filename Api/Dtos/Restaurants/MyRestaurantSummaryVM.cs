using System.Text.Json.Serialization;
using Reservant.Api.Dtos.Location;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Dtos.Restaurants;

/// <summary>
/// Basic info about a restaurant owned by the current user
/// </summary>
public class MyRestaurantSummaryVM
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
    /// NIP
    /// </summary>
    /// <example>1231264550</example>
    public required string Nip { get; init; }

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
    /// Restaurant group ID
    /// </summary>
    public required int GroupId { get; set; }

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
    /// Whether we provide delivery for the restaurant
    /// </summary>
    public required bool ProvideDelivery { get; init; }

    /// <summary>
    /// Restaurant tags
    /// </summary>
    public required List<string> Tags { get; init; }

    /// <summary>
    /// Whether the restaurant is verified or not
    /// </summary>
    public required bool IsVerified { get; init; }

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
