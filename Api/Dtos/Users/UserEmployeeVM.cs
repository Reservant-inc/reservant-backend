using Reservant.Api.Dtos.Employments;

namespace Reservant.Api.Dtos.Users;

/// <summary>
/// Information about a user's employee
/// </summary>
public class UserEmployeeVM
{
    /// <summary>
    /// User ID
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// User login
    /// </summary>
    public required string? Login { get; init; }

    /// <summary>
    /// First name
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// Last name
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// User's birth date
    /// </summary>
    public required DateOnly BirthDate { get; init; }

    /// <summary>
    /// User's phone number
    /// </summary>
    public required string? PhoneNumber { get; init; }

    /// <summary>
    /// Employee's employments
    /// </summary>
    public required List<EmploymentVM>? Employments { get; init; }

    /// <summary>
    /// User's photo path
    /// </summary>
    public required string? Photo { get; init; }

    /// <summary>
    /// Friend status
    /// </summary>
    public required FriendStatus FriendStatus { get; init; }
}
