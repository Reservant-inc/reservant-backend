namespace Reservant.Api.Dtos.User;

/// <summary>
/// Basic info about a user
/// </summary>
public class UserSummaryVM
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
}
