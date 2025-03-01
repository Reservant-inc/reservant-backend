using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Users;

/// <summary>
/// Request to change the current user's information
/// </summary>
public class UpdateUserDetailsRequest
{

    /// <summary>
    /// Full phone number with country code
    /// </summary>
    public required PhoneNumber PhoneNumber { get; init; }

    /// <summary>
    /// First name
    /// </summary>
    /// <example>John</example>
    public required string FirstName { get; init; }

    /// <summary>
    /// Last name
    /// </summary>
    /// <example>Doe</example>
    public required string LastName { get; init; }

    /// <summary>
    /// User's birthdate
    /// </summary>
    /// <example>1999-12-31</example>
    public DateOnly? BirthDate { get; set; }

    /// <summary>
    /// File name of the photo
    /// </summary>
    public string? Photo { get; init; }
}
