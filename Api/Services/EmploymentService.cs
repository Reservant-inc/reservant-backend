using System.ComponentModel.DataAnnotations;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Employment;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

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
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound
            };
        }

        var restaurantOwnerId = employment.Restaurant!.Group!.OwnerId;
        if (restaurantOwnerId != userId)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        employment.DateUntil = DateOnly.FromDateTime(DateTime.Now);
        await context.SaveChangesAsync();

        return true;
    }
    /// <summary>
    /// Terminates all employments specified through ids in the list
    /// </summary>
    /// <param name="employmentIds"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Result<bool>> DeleteBulkEmploymentAsync(List<int> employmentIds, User user)
    {

        var employments = new List<Employment>();
        foreach (var employmentId in employmentIds)
        {
            var employment = await context.Employments
            .Include(e => e.Restaurant)
            .ThenInclude(r => r.Group)
            .FirstOrDefaultAsync(e => e.Id == employmentId && e.DateUntil == null);

            if (employment == null)
            {
                return new ValidationFailure
                {
                    PropertyName = $"{employmentId}",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            if (employment.Restaurant.Group.OwnerId != user.Id)
            {
                return new ValidationFailure
                {
                    PropertyName = $"{employmentId}",
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

            employments.Add(employment);
        }
        foreach (var employment in employments)
        {
            employment.DateUntil = DateOnly.FromDateTime(DateTime.Now);
        }
        await context.SaveChangesAsync();
        return true;
    }
}
