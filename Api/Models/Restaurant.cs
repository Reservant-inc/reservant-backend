using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Reservant.Api.Data;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validation;
using NetTopologySuite.Geometries;

namespace Reservant.Api.Models;

/// <summary>
/// Lokal
/// </summary>
public class Restaurant : ISoftDeletable
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Nazwa
    /// </summary>
    [Required, StringLength(50)]
    public required string Name { get; set; }

    /// <summary>
    /// <a href="https://pl.wikipedia.org/wiki/Numer_identyfikacji_podatkowej">NIP</a> associated with the restaurant
    /// </summary>
    [Required, Nip, StringLength(13)]
    public required string Nip { get; set; }

    /// <summary>
    /// Type of the establishment
    /// </summary>
    public required RestaurantType RestaurantType { get; set; }

    /// <summary>
    /// Adres
    /// </summary>
    [Required, StringLength(70)]
    public required string Address { get; set; }


    /// <summary>
    /// Restaurant location
    /// </summary>
    public required Point Location { get; set; }

    /// <summary>
    /// Postal index of the restaurant
    /// </summary>
    [Required, PostalIndex, StringLength(6)]
    public required string PostalIndex { get; set; }

    /// <summary>
    /// City of the restaurant
    /// </summary>
    [Required, StringLength(15)]
    public required string City { get; set; }

    /// <summary>
    /// File name of the rental contract (umowa najmu lokalu)
    /// </summary>
    [MinLength(1), StringLength(50)]
    public string? RentalContractFileName { get; set; }

    /// <summary>
    /// File name of the alcohol license (licencja na sprzedaż alkoholu)
    /// </summary>
    [MinLength(1), StringLength(50)]
    public string? AlcoholLicenseFileName { get; set; }

    /// <summary>
    /// File name of the permission to conduct business (zgoda na prowadzenie działalności)
    /// </summary>
    [Required, StringLength(50)]
    public required string BusinessPermissionFileName { get; set; }

    /// <summary>
    /// File name of the ID card (dowód osobisty)
    /// </summary>
    [Required, StringLength(50)]
    public required string IdCardFileName { get; set; }

    /// <summary>
    /// Restaurant group ID
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// Whether we provide delivery for the restaurant
    /// </summary>
    public bool ProvideDelivery { get; set; }

    /// <summary>
    /// File name of the logo
    /// </summary>
    [Required, StringLength(50)]
    public required string LogoFileName { get; set; }

    /// <summary>
    /// Optional description of the restaurant
    /// </summary>
    [MinLength(1), StringLength(200)]
    public string? Description { get; set; }

    /// <summary>
    /// Deposit
    /// </summary>
    [Range(0, 500), Column(TypeName = "decimal(5, 2)")]
    public decimal? ReservationDeposit { get; set; }

    /// <summary>
    /// Navigation collection for the tables
    /// </summary>
    public ICollection<Table> Tables { get; set; } = null!;

    /// <summary>
    /// Navigation property for the restaurant group
    /// </summary>
    public RestaurantGroup Group { get; set; } = null!;

    /// <summary>
    /// Navigation property for the rental contract
    /// </summary>
    public FileUpload? RentalContract { get; set; }

    /// <summary>
    /// Navigation property for the alcohol license
    /// </summary>
    public FileUpload? AlcoholLicense { get; set; }

    /// <summary>
    /// Navigation property for the permission to conduct business
    /// </summary>
    public FileUpload BusinessPermission { get; set; } = null!;

    /// <summary>
    /// Navigation property for the ID card
    /// </summary>
    public FileUpload IdCard { get; set; } = null!;

    /// <summary>
    /// Navigation property for the logo
    /// </summary>
    public FileUpload Logo { get; set; } = null!;

    /// <summary>
    /// Navigation collection for the photos. Ordered by the RestaurantPhoto.Order property.
    /// </summary>
    public ICollection<RestaurantPhoto> Photos { get; set; } = null!;

    /// <summary>
    /// Navigation collection for the restaurant tags
    /// </summary>
    public ICollection<RestaurantTag> Tags { get; set; } = null!;

    /// <summary>
    /// Navigational collection for the employees
    /// </summary>
    public ICollection<Employment> Employments { get; set; } = null!;

    /// <summary>
    /// Navigational collection for menus
    /// </summary>
    public ICollection<Menu> Menus { get; set; } = null!;

    /// <summary>
    /// Navigational collection for menu items
    /// </summary>
    public ICollection<MenuItem> MenuItems { get; set; } = null!;

    /// <summary>
    /// Proof of verification by specific CustomerSupportAgent
    /// </summary>
    [StringLength(36)]
    public string? VerifierId { get; set; }

    /// <summary>
    /// Navigational collection for reviews
    /// </summary>
    public ICollection<Review> Reviews { get; set; } = null!;

    /// <summary>
    /// Navigational collection for reviews
    /// </summary>
    public ICollection<Event> Events { get; set; } = null!;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <summary>
    /// get only property for quick-getting rating of the restaurant
    /// </summary>
    public double Rating
    {
        get
        {
            var _res = 0.0;
            foreach (var review in Reviews)
                _res += review.Stars;
            return Reviews.Count > 0 ? _res / Reviews.Count : _res;
        }
    }
}
