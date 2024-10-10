namespace Reservant.Api.Dtos.Users;

/// <summary>
/// Contains information about a user's settings
/// </summary>
public class SettingsDto
{
    /// <summary>
    /// Preferred language
    /// </summary>
    public required string Language { get; set; }
}
