using Reservant.Api.Models.Enums;

namespace Reservant.Api.Dtos.Restaurant;

/// <summary>
/// Request to validate the data given in the first step of creating a restaurant
/// </summary>
public class ValidateRestaurantFirstStepRequest
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
    public RestaurantType RestaurantType { get; init; }

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
    /// Restaurant group ID, if null a new group is created
    /// </summary>
    public int? GroupId { get; set; }
}
