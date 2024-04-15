namespace Reservant.Api.Models.Dtos.Restaurant;

/// <summary>
/// Information about a restaurant employee
/// </summary>
public class RestaurantEmployeeVM
{
    /// <summary>
    /// User ID
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// User login
    /// </summary>
    public required string Login { get; init; }

    /// <summary>
    /// First name
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// Last name
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// Whether the employee is a hall employee (pracownik sali)
    /// </summary>
    public required bool IsHallEmployee { get; init; }

    /// <summary>
    /// Whether the employee is a backdoor employee (pracownik zaplecza)
    /// </summary>
    public required bool IsBackdoorEmployee { get; init; }
}
