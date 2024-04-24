using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.Restaurant;

/// <summary>
/// Request to add an employee to a restaurant
/// </summary>
public class AddEmployeeRequest
{
    /// <summary>
    /// ID of the employee
    /// </summary>
    [Required]
    public required string Id { get; init; }

    /// <summary>
    /// Whether the employee is a hall employee (Pracownik sali)
    /// </summary>
    public bool IsHallEmployee { get; init; }

    /// <summary>
    /// Whether the employee is a back rooms employee (Pracownik zaplecza)
    /// </summary>
    public bool IsBackdoorEmployee { get; init; }
}
