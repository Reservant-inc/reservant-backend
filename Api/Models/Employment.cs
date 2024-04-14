namespace Reservant.Api.Models;

/// <summary>
/// Connects an employee to the restaurants they work at
/// </summary>
public class Employment
{
    /// <summary>
    /// ID of the employee
    /// </summary>
    public required string EmployeeId { get; set; }

    /// <summary>
    /// ID of the restaurant
    /// </summary>
    public int RestaurantId { get; set; }

    /// <summary>
    /// Whether the employee is a hall employee (Pracownik sali)
    /// </summary>
    public bool IsHallEmployee { get; set; }

    /// <summary>
    /// Whether the employee is a backdoor employee (Pracownik zaplecza)
    /// </summary>
    public bool IsBackdoorEmployee { get; set; }

    /// <summary>
    /// Navigational property for the employee
    /// </summary>
    public User? Employee { get; set; }

    /// <summary>
    /// Navigational property for the restaurant
    /// </summary>
    public Restaurant? Restaurant { get; set; }
}
