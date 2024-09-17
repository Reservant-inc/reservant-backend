using ErrorCodeDocs.Attributes;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Employment;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing employmnets
/// </summary>
public class EmploymentService(ApiDbContext context, ValidationService validationService)
{
    /// <summary>
    /// Terminates an employee's employment by setting the end date to the current date.
    /// </summary>
    /// <param name="employmentId">ID of the employment record to terminate.</param>
    /// <param name="userId">ID of the current user, who must be the owner of the restaurant to authorize the termination.</param>
    /// <returns>The bool returned inside the result does not mean anything</returns>
    [ErrorCode(null, ErrorCodes.NotFound)]
    [ErrorCode(null, ErrorCodes.AccessDenied)]
    public async Task<Result> DeleteEmploymentAsync(int employmentId, string userId)
    {
        var employment = await context.Employments
            .Include(e => e.Restaurant)
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
        var restaurantOwnerId = employment.Restaurant.Group.OwnerId;
        if (restaurantOwnerId != userId)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        employment.DateUntil = DateOnly.FromDateTime(DateTime.UtcNow);
        await context.SaveChangesAsync();

        return Result.Success;
    }
    /// <summary>
    /// Terminates all employments specified through ids in the list
    /// </summary>
    /// <param name="employmentIds"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    [ErrorCode("<employmentId>", ErrorCodes.NotFound)]
    [ErrorCode("<employmentId>", ErrorCodes.AccessDenied)]
    public async Task<Result> DeleteBulkEmploymentAsync(List<int> employmentIds, User user)
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
            employment.DateUntil = DateOnly.FromDateTime(DateTime.UtcNow);
        }
        await context.SaveChangesAsync();

        return Result.Success;
    }

    /// <summary>
    /// Updates multilple employments specified in the list
    /// </summary>
    /// <param name="listRequest"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    [ErrorCode(nameof(UpdateEmploymentRequest.EmploymentId), ErrorCodes.NotFound)]
    public async Task<Result> UpdateBulkEmploymentAsync(List<UpdateEmploymentRequest> listRequest, User user)
    {
        foreach (var request in listRequest)
        {
            var res = await validationService.ValidateAsync(request, user.Id);
            if (!res.IsValid)
            {
                return res;
            }
            var employment = context.Employments.Include(e => e.Restaurant)
                .ThenInclude(r => r.Group)
                .FirstOrDefault(e => e.Id == request.EmploymentId && e.Restaurant.Group.OwnerId == user.Id);

            if (employment is null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(request.EmploymentId),
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            employment.IsBackdoorEmployee = request.IsBackdoorEmployee;
            employment.IsHallEmployee = request.IsHallEmployee;
        }

        await context.SaveChangesAsync();
        return Result.Success;
    }
}
