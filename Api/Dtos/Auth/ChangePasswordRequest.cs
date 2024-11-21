namespace Reservant.Api.Dtos.Auth;

/// <summary>
/// Request to change the current user's password
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// User's current password
    /// </summary>
    /// <example>Pa$$w0rd</example>
    public required string OldPassword { get; init; }

    /// <summary>
    /// New password to set
    /// </summary>
    /// <example>Pa$$w0rd</example>
    public required string NewPassword { get; init; }
}
