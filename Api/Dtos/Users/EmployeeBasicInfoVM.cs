namespace Reservant.Api.Dtos.Users;

/// <summary>
/// Basic information about a restaurant employee
/// </summary>
public class EmployeeBasicInfoVM
{
    /// <summary>
    /// Employee ID
    /// </summary>
    public required Guid EmployeeId { get; init; }

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
