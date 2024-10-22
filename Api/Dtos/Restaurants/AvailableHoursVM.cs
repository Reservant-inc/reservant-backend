namespace Reservant.Api.Dtos.Restaurants;

/// <summary>
/// View model that represents an available time slot for reservations at a restaurant.
/// </summary>
public class AvailableHoursVM
{
    /// <summary>
    /// Available from
    /// </summary>
    public TimeSpan From { get; set; }

    /// <summary>
    /// Available until
    /// </summary>
    public TimeSpan Until { get; set; }
}
