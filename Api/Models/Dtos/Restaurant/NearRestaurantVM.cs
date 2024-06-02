using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Reservant.Api.Models.Dtos.Location;
using Reservant.Api.Models.Dtos.Table;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validation;

namespace Reservant.Api.Models.Dtos.Restaurant;

public class NearRestaurantVM
{
        /// <summary>
    /// Unique ID
    /// </summary>
    [Required]
    public required int RestaurantId { get; init; }

    /// <summary>
    /// Nazwa
    /// </summary>
    /// <example>McJohn's</example>
    [Required, StringLength(50)]
    public required string Name { get; init; }

    /// <summary>
    /// Type of the establishment
    /// </summary>
    public required RestaurantType RestaurantType { get; set; }

    /// <summary>
    /// <a href="https://pl.wikipedia.org/wiki/Numer_identyfikacji_podatkowej">NIP</a> associated with the restaurant
    /// </summary>
    /// <example>1231264550</example>
    [Required, Nip]
    public required string Nip { get; init; }

    /// <summary>
    /// Adres
    /// </summary>
    /// <example>ul. Koszykowa 86</example>
    [Required, StringLength(70)]
    public required string Address { get; init; }

    /// <summary>
    /// Postal index of the restaurant
    /// </summary>
    /// <example>00-000</example>
    [Required, PostalIndex]
    public required string PostalIndex { get; init; }

    /// <summary>
    /// City of the restaurant
    /// </summary>
    /// <example>Warszawa</example>
    [Required, StringLength(15)]
    public required string City { get; init; }

    /// <summary>
    /// Geolocation class having Longitude, Latitude
    /// </summary>
    /// <example></example>
    [Required]
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
    [MinLength(1), StringLength(50)]
    public required string? RentalContract { get; set; }

    /// <summary>
    /// URI of the alcohol license (licencja na sprzedaż alkoholu)
    /// </summary>
    [MinLength(1), StringLength(50)]
    public required string? AlcoholLicense { get; set; }

    /// <summary>
    /// URI of the permission to conduct business (zgoda na prowadzenie działalności)
    /// </summary>
    [Required, StringLength(50)]
    public required string BusinessPermission { get; set; }

    /// <summary>
    /// URI of the ID card (dowód osobisty)
    /// </summary>
    [Required, StringLength(50)]
    public required string IdCard { get; set; }

    /// <summary>
    /// List of tables in the restaurant
    /// </summary>
    [Required]
    public required IEnumerable<TableVM> Tables { get; init; }

    /// <summary>
    /// Whether we provide delivery for the restaurant
    /// </summary>
    public required bool ProvideDelivery { get; init; }

    /// <summary>
    /// URI of the logo
    /// </summary>
    [Required, StringLength(50)]
    public required string Logo { get; init; }

    /// <summary>
    /// Photos of the restaurant
    /// </summary>
    [Required]
    public required List<string> Photos { get; init; }

    /// <summary>
    /// Optional description of the restaurant
    /// </summary>
    [MinLength(1), StringLength(200)]
    public required string? Description { get; init; }

    /// <summary>
    /// Deposit
    /// </summary>
    [Range(0, 500), Column(TypeName = "decimal(5, 2)")]
    public required decimal? ReservationDeposit { get; init; }

    /// <summary>
    /// Restaurant tags
    /// </summary>
    [Required]
    public required List<string> Tags { get; init; }

    /// <summary>
    /// Whether the restaurant is verified or not
    /// </summary>
    [Required]
    public required bool IsVerified { get; init; }
    
    /// <summary>
    /// Distance from User
    /// </summary>
    [Required]
    public required double distanceFrom { get; init; }
}