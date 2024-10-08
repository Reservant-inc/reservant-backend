namespace Reservant.Api.Dtos.User;

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
