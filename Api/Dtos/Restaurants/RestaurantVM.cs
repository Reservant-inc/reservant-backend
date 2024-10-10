using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validation;
using Reservant.Api.Dtos.Location;
using Reservant.Api.Dtos.Tables;

namespace Reservant.Api.Dtos.Restaurants;

/// <summary>
/// Publicly available detailed information about a Restaurant
/// </summary>
public class RestaurantVM
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
    /// <example>ul. Koszykowa 86</example>
    public required string Address { get; init; }

    /// <summary>
    /// Postal index of the restaurant
    /// </summary>
    /// <example>00-000</example>
    public required string PostalIndex { get; init; }

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
    /// List of tables in the restaurant
    /// </summary>
    public required IEnumerable<TableVM> Tables { get; init; }

    /// <summary>
    /// Whether we provide delivery for the restaurant
    /// </summary>
    public required bool ProvideDelivery { get; init; }

    /// <summary>
    /// URI of the logo
    /// </summary>
    public required string Logo { get; init; }

    /// <summary>
    /// Photos of the restaurant
    /// </summary>
    public required List<string> Photos { get; init; }

    /// <summary>
    /// Optional description of the restaurant
    /// </summary>
    public required string? Description { get; init; }

    /// <summary>
    /// Deposit
    /// </summary>
    public required decimal? ReservationDeposit { get; init; }

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
}
