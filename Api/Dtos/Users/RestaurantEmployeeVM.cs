namespace Reservant.Api.Dtos.Users;

/// <summary>
/// Information about a restaurant employee
/// </summary>
public class RestaurantEmployeeVM
{
    /// <summary>
    /// Employee ID
    /// </summary>
    public required Guid EmployeeId { get; init; }

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
    /// Birth date
    /// </summary>
    public required DateOnly BirthDate { get; init; }

    /// <summary>
    /// User's phone number
    /// </summary>
    public required string PhoneNumber { get; init; }

    /// <summary>
    /// Whether the employee is a hall employee (pracownik sali)
    /// </summary>
    public required bool IsHallEmployee { get; init; }

    /// <summary>
    /// Whether the employee is a backdoor employee (pracownik zaplecza)
    /// </summary>
    public required bool IsBackdoorEmployee { get; init; }

    /// <summary>
    /// Start date of the employment.
    /// </summary>
    public required DateOnly DateFrom { get; init; }

    /// <summary>
    /// End date of the employment, if applicable.
    /// </summary>
    public required DateOnly? DateUntil { get; init; }

    /// <summary>
    /// Employment ID.
    /// </summary>
    public required int EmploymentId { get; init; }
}
