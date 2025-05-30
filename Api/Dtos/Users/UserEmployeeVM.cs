using Reservant.Api.Dtos.Employments;
using Reservant.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Data;

namespace Reservant.Api.Dtos.Users;

/// <summary>
/// Information about a user's employee
/// </summary>
public class UserEmployeeVM
{
    /// <summary>
    /// User ID
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// User login
    /// </summary>
    public required string? Login { get; set; }

    /// <summary>
    /// First name
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// Last name
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// User's birthdate
    /// </summary>
    public required DateOnly BirthDate { get; init; }


    /// <summary>
    /// Full phone number with country code
    /// </summary>
    public required PhoneNumber? PhoneNumber { get; set; }

    /// <summary>
    /// Employee's employments
    /// </summary>
    public required List<EmploymentVM>? Employments { get; set; }
    
    /// <summary>
    /// Is the user friends with the current user?
    /// </summary>
    public required FriendStatus? FriendStatus { get; set; }

    /// <summary>
    /// User's photo path
    /// </summary>
    public required string? Photo { get; init; }

    /// <summary>
    /// Precise time of user beeing unbanned, null if not banned
    /// </summary>
    public required DateTime? BannedUntil { get; set; }
}
