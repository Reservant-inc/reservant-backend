namespace Reservant.Api.Models.Dtos.Employment;

/// <summary>
/// Information about a user's employment at a restaurant
/// </summary>
public class EmploymentVM
{
    /// <summary>
    /// ID of the restaurant
    /// </summary>
    public required int RestaurantId { get; init; }

    /// <summary>
    /// Whether the employee is a backdoor employee
    /// </summary>
    public required bool IsBackdoorEmployee { get; init; }

    /// <summary>
    /// Whether the employee is a hall employee
    /// </summary>
    public required bool IsHallEmployee { get; init; }
}
