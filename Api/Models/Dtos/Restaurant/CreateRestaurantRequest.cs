using System.ComponentModel.DataAnnotations;
using Reservant.Api.Validation;

namespace Reservant.Api.Models.Dtos.Restaurant;

/// <summary>
/// Information about the restaurant to create
/// </summary>
public class CreateRestaurantRequest
{
    /// <summary>
    /// Name of the restaurant
    /// </summary>
    /// <example>McJohn's</example>
    [Required, StringLength(50)]
    public required string Name { get; init; }

    /// <summary>
    /// <a href="https://pl.wikipedia.org/wiki/Numer_identyfikacji_podatkowej">NIP</a> associated with the restaurant
    /// </summary>
    /// <example>022-41-11-111</example>
    [Required, Nip]
    public required string Nip { get; init; }

    /// <summary>
    /// Address of the restaurant
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
    /// Restaurant group ID, if null a new group is created
    /// </summary>
    public int? GroupId { get; set; }
}
