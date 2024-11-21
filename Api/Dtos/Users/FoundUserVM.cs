using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Users;

/// <summary>
/// Information about a user found using search
/// </summary>
public class FoundUserVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// First name
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// Last name
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// Profile picture path
    /// </summary>
    public required string? Photo { get; init; }

    /// <summary>
    /// Is the user friends with the current user?
    /// </summary>
    public required FriendStatus FriendStatus { get; init; }

    /// <summary>
    /// Full phone number with country code
    /// </summary>
    public required PhoneNumber? PhoneNumber { get; init; }
}
