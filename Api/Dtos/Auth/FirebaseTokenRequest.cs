namespace Reservant.Api.Dtos.Auth;

/// <summary>
/// Request to register or unregister an Firebase device token
/// </summary>
public class FirebaseTokenRequest
{
    /// <summary>
    /// The Firebase device token
    /// </summary>
    public required string DeviceToken { get; init; }
}
