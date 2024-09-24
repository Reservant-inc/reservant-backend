using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Reservant.Api.Dtos.Location;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validation;

namespace Reservant.Api.Dtos.Restaurant;

/// <summary>
/// Request to update information about a restaurants
/// </summary>
public class UpdateRestaurantRequest
{
    /// <summary>
    /// Name of the restaurant
    /// </summary>
    /// <example>McJohn's</example>
    public required string Name { get; init; }

    /// <summary>
    /// <a href="https://pl.wikipedia.org/wiki/Numer_identyfikacji_podatkowej">NIP</a> associated with the restaurant
    /// </summary>
    /// <example>1231264550</example>
    public required string Nip { get; init; }

    /// <summary>
    /// Restaurant type
    /// </summary>
    public required RestaurantType RestaurantType { get; init; }

    /// <summary>
    /// Address of the restaurant
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
    /// File name of the rental contract upload (umowa najmu lokalu)
    /// </summary>
    /// <example>306f9fa1-fda5-48c4-aa5f-7c7c375e065f.pdf</example>
    public string? RentalContract { get; init; }

    /// <summary>
    /// File name of the alcohol license upload (licencja na sprzedaż alkoholu)
    /// </summary>
    /// <example>306f9fa1-fda5-48c4-aa5f-7c7c375e065f.pdf</example>
    public string? AlcoholLicense { get; init; }

    /// <summary>
    /// File name of the permission to conduct business upload (zgoda na prowadzenie działalności)
    /// </summary>
    /// <example>306f9fa1-fda5-48c4-aa5f-7c7c375e065f.pdf</example>
    public required string BusinessPermission { get; init; }

    /// <summary>
    /// File name of the ID card upload (dowód osobisty)
    /// </summary>
    /// <example>306f9fa1-fda5-48c4-aa5f-7c7c375e065f.pdf</example>
    public required string IdCard { get; init; }

    /// <summary>
    /// File name of the logo upload
    /// </summary>
    /// <example>306f9fa1-fda5-48c4-aa5f-7c7c375e065f.png</example>
    public required string Logo { get; init; }

    /// <summary>
    /// Whether we provide delivery for the restaurant
    /// </summary>
    public bool ProvideDelivery { get; init; }

    /// <summary>
    /// Optional description of the restaurant
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Deposit
    /// </summary>
    public decimal? ReservationDeposit { get; init; }

    /// <summary>
    /// Restaurant tags
    /// </summary>
    public required HashSet<string> Tags { get; init; }

    /// <summary>
    /// Restaurant photos
    /// </summary>
    public required List<string> Photos { get; init; }
}
