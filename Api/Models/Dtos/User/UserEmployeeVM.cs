using Reservant.Api.Models.Dtos.Employment;

namespace Reservant.Api.Models.Dtos.User;

/// <summary>
/// Information about a user's employee
/// </summary>
public class UserEmployeeVM
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
    /// User's phone number
    /// </summary>
    public required string PhoneNumber { get; init; }

    /// <summary>
    /// Employee's employments
    /// </summary>
    public required List<EmploymentVM> Employments { get; init; }
}
