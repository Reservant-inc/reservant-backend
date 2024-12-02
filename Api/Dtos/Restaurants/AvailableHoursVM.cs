namespace Reservant.Api.Dtos.Restaurants;

/// <summary>
/// View model that represents an available time slot for reservations at a restaurant.
/// </summary>
public class AvailableHoursVM
{
    /// <summary>
    /// Available from
    /// </summary>
    public TimeOnly? From { get; set; }

    /// <summary>
    /// Available until
    /// </summary>
    public TimeOnly? Until { get; set; }
}
