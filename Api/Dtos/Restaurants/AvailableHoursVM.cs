using Reservant.Api.Models;

namespace Reservant.Api.Dtos.Restaurants;

/// <summary>
/// View model that represents the available time slots for reservations at a restaurant.
/// </summary>
public class AvailableHoursVM
{
    /// <summary>
    /// A list of available time slots during which tables can be reserved at the restaurant.
    /// Each time slot is represented by a <see cref="AvailableHours"/> object, 
    /// which contains the start and end time for the availability.
    /// </summary>
    public List<AvailableHours> AvailableHours { get; set; } = new();
}
