namespace Reservant.Api.Dtos.Employment;

/// <summary>
/// Request to update an Employment
/// </summary>
public class UpdateEmploymentRequest
{
    /// <summary>
    /// Id of the employment
    /// </summary>
    public required int EmploymentId { get; set; }
    /// <summary>
    /// Whether the employee is a hall employee (Pracownik sali)
    /// </summary>
    public required bool IsHallEmployee { get; set; }

    /// <summary>
    /// Whether the employee is a backdoor employee (Pracownik zaplecza)
    /// </summary>
    public required bool IsBackdoorEmployee { get; set; }
}
