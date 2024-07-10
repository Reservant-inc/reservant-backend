using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Reservant.Api.Models.Dtos.Location;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validation;

namespace Reservant.Api.Models.Dtos.Restaurant;

/// <summary>
/// Info about a restaurant near user
/// </summary>
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
    /// NIP
    /// </summary>
    /// <example>1231264550</example>
    [Required, Nip, StringLength(13)]
    public required string Nip { get; init; }

    /// <summary>
    /// Type of the establishment
    /// </summary>
    public required RestaurantType RestaurantType { get; set; }

    /// <summary>
    /// Adres
    /// </summary>
    ///  <example>ul. Koszykowa 86</example>
    [Required, StringLength(70)]
    public required string Address { get; init; }

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
    /// URI of the logo
    /// </summary>
    [Required]
    public required string Logo { get; init; }

    /// <summary>
    /// Optional description
    /// </summary>
    [StringLength(200)]
    public required string? Description { get; init; }

    /// <summary>
    /// Deposit
    /// </summary>
    [Range(0, 500), Column(TypeName = "decimal(5, 2)")]
    public required decimal? ReservationDeposit { get; init; }

    /// <summary>
    /// Whether we provide delivery for the restaurant
    /// </summary>
    public required bool ProvideDelivery { get; init; }

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
    public required double DistanceFrom { get; init; }

    /// <summary>
    /// Rating of the restaurant based on the reviews
    /// </summary>
    [Required]
    public required decimal Rating { get; set; }

    /// <summary>
    /// Number of reviews about this restaurant
    /// </summary>
    [Required]
    public required int NumberReviews { get; set; }
}
