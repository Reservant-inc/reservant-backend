namespace Reservant.Api.Dtos.Employments;

/// <summary>
/// Information about a user's employment at a restaurant
/// </summary>
public class EmploymentVM
{
    /// <summary>
    /// ID of Employment
    /// </summary>
    public required int EmploymentId { get; init; }

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

    /// <summary>
    /// The name of the restaurant that employes the employee
    /// </summary>
    public required string RestaurantName { get; init; }

    /// <summary>
    /// Date from which employment started
    /// </summary>
    public required DateOnly DateFrom { get; init; }
}
