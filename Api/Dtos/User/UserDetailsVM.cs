using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Dtos.User;

/// <summary>
/// Information about the current user
/// </summary>
public class UserDetailsVM
{
    /// <summary>
    /// User ID
    /// </summary>
    /// <example>e5779baf-5c9b-4638-b9e7-ec285e57b367</example>
    public required Guid UserId { get; init; }

    /// <summary>
    /// User's login
    /// </summary>
    /// <example>JD</example>
    public required string Login { get; init; }

    /// <summary>
    /// User's email address
    /// </summary>
    /// <example>john@doe.pl</example>
    public required string Email { get; init; }

    /// <summary>
    /// User's phone number
    /// </summary>
    /// <example>+48123456789</example>
    public required string PhoneNumber { get; init; }

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
    /// When the account was created
    /// </summary>
    public required DateTime RegisteredAt { get; set; }

    /// <summary>
    /// User's birthdate
    /// </summary>
    /// <example>1999-12-31</example>
    public DateOnly? BirthDate { get; set; }

    /// <summary>
    /// User's roles
    /// </summary>
    public required List<string> Roles { get; init; }

    /// <summary>
    /// ID of the RestaurantOwner who employs the user. For restaurant employees
    /// </summary>
    /// <example>e5779baf-5c9b-4638-b9e7-ec285e57b367</example>
    public Guid? EmployerId { get; set; }

    /// <summary>
    /// User's profile picture
    /// </summary>
    public required string? Photo { get; set; }
}
