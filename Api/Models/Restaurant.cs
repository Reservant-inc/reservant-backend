using System.ComponentModel.DataAnnotations;
using Reservant.Api.Validation;

namespace Reservant.Api.Models;

/// <summary>
/// Lokal
/// </summary>
public class Restaurant
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
    /// Restaurant group ID
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// Navigation collection for the tables
    /// </summary>
    public ICollection<Table>? Tables { get; set; }

    /// <summary>
    /// Navigation property for the restaurant group
    /// </summary>
    public RestaurantGroup? Group { get; set; }
}
