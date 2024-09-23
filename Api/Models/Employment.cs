using Reservant.Api.Data;
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models;

/// <summary>
/// Connects an employee to the restaurants they work at
/// </summary>
public class Employment : ISoftDeletable
{
    /// <summary>
    /// Unique identifier for the employment record.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// ID of the employee
    /// </summary>
    [StringLength(36)]
    public string EmployeeId { get; set; } = null!;

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
    /// The start date of employment.
    /// </summary>
    public DateOnly DateFrom { get; set; }

    /// <summary>
    /// The end date of employment, if applicable.
    /// </summary>
    public DateOnly? DateUntil { get; set; }

    /// <summary>
    /// Navigational property for the employee
    /// </summary>
    public User Employee { get; set; } = null!;

    /// <summary>
    /// Navigational property for the restaurant
    /// </summary>
    public Restaurant Restaurant { get; set; } = null!;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }
}
