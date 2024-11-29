namespace Reservant.Api.Dtos.Auth;

/// <summary>
/// Dto returned when a banned user tries to log in
/// </summary>
public class YouAreBannedDto
{
    /// <summary>
    /// Time until which the user is banned
    /// </summary>
    public required DateTime BannedUntil { get; set; }
}
