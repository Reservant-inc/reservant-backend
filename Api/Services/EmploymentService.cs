using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Validation;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing employmnets
/// </summary>
public class EmploymentService(ApiDbContext context)
{
    /// <summary>
    /// Terminates an employee's employment by setting the end date to the current date.
    /// </summary>
    /// <param name="employmentId">ID of the employment record to terminate.</param>
    /// <param name="userId">ID of the current user, who must be the owner of the restaurant to authorize the termination.</param>
    /// <returns>The bool returned inside the result does not mean anything</returns>
    public async Task<Result<bool>> DeleteEmploymentAsync(int employmentId, string userId)
    {
        var employment = await context.Employments
            .Include(e => e.Restaurant!)
            .ThenInclude(r => r.Group)
            .FirstOrDefaultAsync(e => e.Id == employmentId && e.DateUntil == null);

        if (employment == null)
        {
            return new List<ValidationResult> {
                new ValidationResult($"Employment with ID {employmentId} not found or already terminated")
            };
        }

        var restaurantOwnerId = employment.Restaurant!.Group!.OwnerId;
        if (restaurantOwnerId != userId)
        {
            return new List<ValidationResult>
            {
                new("User is not the owner of the restaurant")
            };
        }

        employment.DateUntil = DateOnly.FromDateTime(DateTime.Now);
        await context.SaveChangesAsync();

        return true;
    }
}
