namespace Reservant.Api.Models;

/// <summary>
/// Opening hours of a restaurant on a particular day
/// </summary>
public class OpeningHours
{
    /// <summary>
    /// When the restaurant opens
    /// </summary>
    public TimeOnly? From { get; set; }

    /// <summary>
    /// When the restaurant closes
    /// </summary>
    public TimeOnly? Until { get; set; }
}
