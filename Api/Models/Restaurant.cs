using System.ComponentModel.DataAnnotations;
using Reservant.Api.Data;
using Reservant.Api.Validation;

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
    /// Navigation collection for the tables
    /// </summary>
    public ICollection<Table>? Tables { get; set; }

    /// <summary>
    /// Navigation property for the restaurant group
    /// </summary>
    public RestaurantGroup? Group { get; set; }

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
    public FileUpload? BusinessPermission { get; set; }

    /// <summary>
    /// Navigation property for the ID card
    /// </summary>
    public FileUpload? IdCard { get; set; }

    /// <summary>
    /// Navigation property for the logo
    /// </summary>
    public FileUpload? Logo { get; set; }

    /// <summary>
    /// Navigation collection for the photos. Ordered by the RestaurantPhoto.Order property.
    /// </summary>
    public ICollection<RestaurantPhoto>? Photos { get; set; }

    /// <summary>
    /// Navigation collection for the restaurant tags
    /// </summary>
    public ICollection<RestaurantTag>? Tags { get; set; }

    /// <summary>
    /// Navigational collection for the employees
    /// </summary>
    public ICollection<Employment>? Employments { get; set; }

    /// <summary>
    /// Proof of verification by specific CustomerSupportAgent
    /// </summary>
    public string? VerifierId { get; set; }

    /// <inheritdoc />
    public bool IsDeleted { get; set; } = false; //Default false
}
