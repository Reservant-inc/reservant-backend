namespace Reservant.Api.Dtos.Users;

/// <summary>
/// Contains information about a ban
/// </summary>
public class BanDto
{
    /// <summary>
    /// Duration of ban
    /// </summary>
    public required TimeSpan timeSpan { get; set; }
}   
