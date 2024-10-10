using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validation;
using Reservant.Api.Dtos.Location;
using Reservant.Api.Dtos.Tables;

namespace Reservant.Api.Dtos.Restaurants;

/// <summary>
/// Detailed information about a Restaurant that the user owns
/// </summary>
public class MyRestaurantVM
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
    /// <a href="https://pl.wikipedia.org/wiki/Numer_identyfikacji_podatkowej">NIP</a> associated with the restaurant
    /// </summary>
    /// <example>1231264550</example>
    public required string Nip { get; init; }

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
    /// Restaurant group ID
    /// </summary>
    public required int GroupId { get; set; }

    /// <summary>
    /// Restaurant group name
    /// </summary>
    /// <example>McJohn's Restaurant Group</example>
    public required string GroupName { get; set; }

    /// <summary>
    /// URI of the rental contract (umowa najmu lokalu)
    /// </summary>
    public required string? RentalContract { get; set; }

    /// <summary>
    /// URI of the alcohol license (licencja na sprzedaż alkoholu)
    /// </summary>
    public required string? AlcoholLicense { get; set; }

    /// <summary>
    /// URI of the permission to conduct business (zgoda na prowadzenie działalności)
    /// </summary>
    public required string BusinessPermission { get; set; }

    /// <summary>
    /// URI of the ID card (dowód osobisty)
    /// </summary>
    public required string IdCard { get; set; }

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
    /// Whether the restaurant is verified or not
    /// </summary>
    public required bool IsVerified { get; init; }
}
