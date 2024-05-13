namespace Reservant.Api.Models.Dtos.Employment;

/// <summary>
/// Request to update an Employment
/// </summary>
public class UpdateEmploymentRequest
{
    /// <summary>
    /// Whether the employee is a hall employee (Pracownik sali)
    /// </summary>
    public bool IsHallEmployee { get; set; }

    /// <summary>
    /// Whether the employee is a backdoor employee (Pracownik zaplecza)
    /// </summary>
    public bool IsBackdoorEmployee { get; set; }
}
