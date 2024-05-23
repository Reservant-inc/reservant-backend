using System.ComponentModel.DataAnnotations;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validation;

namespace Reservant.Api.Models.Dtos.Restaurant;

/// <summary>
/// Request to validate the data given in the first step of creating a restaurant
/// </summary>
public class ValidateRestaurantFirstStepRequest
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
    /// <example>1231264550</example>
    [Required, Nip]
    public required string Nip { get; init; }

    /// <summary>
    /// Restaurant type
    /// </summary>
    public RestaurantType RestaurantType { get; init; }

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
