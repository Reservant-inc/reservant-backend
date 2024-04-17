using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.User;

/// <summary>
/// Information about the current user
/// </summary>
public class UserDetailsVM
{
    /// <summary>
    /// User ID
    /// </summary>
    /// <example>e5779baf-5c9b-4638-b9e7-ec285e57b367</example>
    [Required]
    public required string Id { get; init; }

    /// <summary>
    /// User's login
    /// </summary>
    /// <example>JD</example>
    [Required]
    public required string Login { get; init; }

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
    /// When the account was created
    /// </summary>
    [Required]
    public required DateTime RegisteredAt { get; set; }

    /// <summary>
    /// User's birthdate
    /// </summary>
    /// <example>1999-12-31</example>
    public DateOnly? BirthDate { get; set; }

    /// <summary>
    /// User's roles
    /// </summary>
    [Required]
    public required List<string> Roles { get; init; }

    /// <summary>
    /// ID of the RestaurantOwner who employs the user. For restaurant employees
    /// </summary>
    /// <example>e5779baf-5c9b-4638-b9e7-ec285e57b367</example>
    public string? EmployerId { get; set; }
}
