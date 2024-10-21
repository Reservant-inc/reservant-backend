using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Reservant.Api.Dtos.Location;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validation;

namespace Reservant.Api.Dtos.Restaurants;

/// <summary>
/// Info about a restaurant near user
/// </summary>
public class NearRestaurantVM
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
    /// Whether we provide delivery for the restaurant
    /// </summary>
    public required bool ProvideDelivery { get; init; }

    /// <summary>
    /// Restaurant tags
    /// </summary>
    public required List<string> Tags { get; init; }

    /// <summary>
    /// Hours when the restaurant is open
    /// </summary>
    public required AvailableHoursVM OpeningHours { get; set; }

    /// <summary>
    /// Distance from User
    /// </summary>
    public required double? DistanceFrom { get; init; }

    /// <summary>
    /// Rating of the restaurant based on the reviews
    /// </summary>
    public required double Rating { get; set; }

    /// <summary>
    /// Number of reviews about this restaurant
    /// </summary>
    public required int NumberReviews { get; set; }
}
