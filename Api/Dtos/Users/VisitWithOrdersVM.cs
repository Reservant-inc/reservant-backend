using Reservant.Api.Dtos.Employments;
using Reservant.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Visits;
using Reservant.Api.Dtos.Orders;

namespace Reservant.Api.Dtos.Users;

/// <summary>
/// Information about a user's employee
/// </summary>
public class VisitWithOrdersVM
{
    /// <summary>
    /// User ID
    /// </summary>
    public required VisitVM visitVM { get; set; }

    /// <summary>
    /// User ID
    /// </summary>
    public required ICollection<OrderVM> Orders { get; set; }
}
