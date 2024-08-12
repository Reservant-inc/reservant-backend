using Microsoft.AspNetCore.Identity;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Validation;

namespace Reservant.Api.Services;

/// <summary>
/// Service for deliveries
/// </summary>
public class DeliveryService(
    UserManager<User> userManager,
    ApiDbContext context,
    ValidationService validationService)
{

}
