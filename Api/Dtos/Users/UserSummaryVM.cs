using System.Text.Json.Serialization;

namespace Reservant.Api.Dtos.Users;

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

    /// <summary>
    /// Whether the restaurant is archived
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public required bool IsArchived { get; init; }
}
