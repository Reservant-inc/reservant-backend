using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.User;

/// <summary>
/// Request to change the current user's information
/// </summary>
public class UpdateUserDetailsRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    /// <example>john@doe.pl</example>
    [Required]
    public required string Email { get; init; }

    /// <summary>
    /// User's phone number
    /// </summary>
    /// <example>+48123456789</example>
    [Required]
    public required string PhoneNumber { get; init; }

    /// <summary>
    /// First name
    /// </summary>
    /// <example>John</example>
    [Required]
    public required string FirstName { get; init; }

    /// <summary>
    /// Last name
    /// </summary>
    /// <example>Doe</example>
    [Required]
    public required string LastName { get; init; }

    /// <summary>
    /// User's birthdate
    /// </summary>
    /// <example>1999-12-31</example>
    public DateOnly? BirthDate { get; set; }
}
