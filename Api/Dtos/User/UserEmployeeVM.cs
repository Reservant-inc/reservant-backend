using Reservant.Api.Dtos.Employment;

namespace Reservant.Api.Dtos.User;

/// <summary>
/// Information about a user's employee
/// </summary>
public class UserEmployeeVM
{
    /// <summary>
    /// User ID
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// User login
    /// </summary>
    public string? Login { get; init; }

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
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Employee's employments
    /// </summary>
    public List<EmploymentVM>? Employments { get; init; }

    /// <summary>
    /// User's photo path
    /// </summary>
    public string? Photo { get; init; }

    /// <summary>
    /// Friend status
    /// </summary>
    public FriendStatus? FriendStatus { get; init; }
}
