using AutoMapper;
using Reservant.Api.Dtos.Restaurants;

namespace Reservant.Api.Dtos.Employments;

/// <summary>
/// Information about a user's employment at a restaurant
/// </summary>
[AutoMap(typeof(EmploymentVM))]
public class EmploymentVM
{
    /// <summary>
    /// ID of Employment
    /// </summary>
    public required int EmploymentId { get; init; }

    /// <summary>
    /// Summary of the restaurant details
    /// </summary>
    public required RestaurantSummaryVM Restaurant { get; init; }

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

    /// <summary>
    /// Date until the employment was active, null if it still is
    /// </summary>
    public required DateOnly? DateUntil { get; init; }
}
