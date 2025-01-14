using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services.VisitServices;

/// <summary>
/// Service responsible for updating the table assigned to a visit
/// </summary>
/// <param name="context"></param>
/// <param name="authorizationService"></param>
public class UpdateVisitTableService(
    ApiDbContext context,
    AuthorizationService authorizationService)
{
    /// <summary>
    /// Updates the table assigned to a visit.
    /// </summary>
    /// <param name="visitId">ID of the visit to update.</param>
    /// <param name="newTableId">ID of the new table to assign to the visit.</param>
    /// <param name="currentUserId">ID of the current user for permission checks.</param>
    /// <returns>
    /// <c>Result.Success</c> if the table was successfully updated; otherwise, an error result:
    /// <list type="bullet">
    /// <item><c>NotFound</c>: Visit or table not found.</item>
    /// <item><c>AccessDenied</c>: User does not have permission to modify the visit.</item>
    /// <item><c>IncorrectVisitStatus</c>: The visit has already ended.</item>
    /// <item><c>TableNotAvailable</c>: The table is not available for the duration of the visit.</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The method checks if the table is available for the time range defined by the visit:
    /// <list type="number">
    /// <item>If the visit has started, the start time is set to the current time; otherwise, it uses the reservation start time.</item>
    /// <item>If the visit has a reservation, the end time is determined by the reservation end time.
    /// If it does not have a reservation, the end time is calculated as the start time plus the restaurant's maximum duration for a visit.</item>
    /// </list>
    /// </remarks>
    [ErrorCode(nameof(visitId), ErrorCodes.NotFound)]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyRestaurantHallAccess))]
    [ErrorCode(null, ErrorCodes.IncorrectVisitStatus)]
    [ErrorCode(nameof(newTableId), ErrorCodes.NotFound)]
    [ErrorCode(null, ErrorCodes.InvalidState,
        "Invalid state of the visit: visit has not started and there is no reservation")]
    [ErrorCode(nameof(newTableId), ErrorCodes.TableNotAvailable)]
    public async Task<Result> UpdateVisitTableAsync(int visitId, int newTableId, Guid currentUserId)
    {
        var visit = await FindVisitIncludingRestaurant(visitId);
        if (visit is null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(visitId),
                ErrorMessage = "Visit not found",
                ErrorCode = ErrorCodes.NotFound,
            };
        }

        var authResult = await authorizationService.VerifyRestaurantHallAccess(visit.RestaurantId, currentUserId);
        if (authResult.IsError) return authResult;

        if (visit.HasEnded())
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Cannot change table for a visit that has already ended",
                ErrorCode = ErrorCodes.IncorrectVisitStatus,
            };
        }

        var newTable = await FindTableInRestaurant(newTableId, visit.RestaurantId);
        if (newTable is null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(newTableId),
                ErrorMessage = "Table not found in selected restaurant",
                ErrorCode = ErrorCodes.NotFound,
            };
        }

        if (await OverlapsWithAnotherVisitIfTableIsChanged(visit, newTable))
        {
            return new ValidationFailure
            {
                PropertyName = nameof(newTableId),
                ErrorMessage = "The table is nov available in selected time period",
                ErrorCode = ErrorCodes.TableNotAvailable,
            };
        }

        visit.TableId = newTableId;
        await context.SaveChangesAsync();

        return Result.Success;
    }

    /// <summary>
    /// Check if the visit would overlap with any other visit if the table was changed
    /// </summary>
    /// <param name="visit"></param>
    /// <param name="newTable"></param>
    /// <returns></returns>
    private async Task<bool> OverlapsWithAnotherVisitIfTableIsChanged(Visit visit, Table newTable)
    {
        var currentTime = DateTime.Now;

        var (visitStartTime, visitEndTime) = GetTimeIntervalToCheck(visit, currentTime);
        var potentialOverlappingVisits = await GetPossibleCollidingVisits(visit, newTable);
        return potentialOverlappingVisits.Any(v =>
        {
            var (otherVisitStartTime, otherVisitEndTime) = GetTimeIntervalToCheck(v, currentTime);
            return (otherVisitStartTime < visitEndTime) && (otherVisitEndTime > visitStartTime);
        });
    }

    /// <summary>
    /// Get the time interval within which a visit might overlap with others if the table was changed
    /// </summary>
    /// <param name="visit">The visit</param>
    /// <param name="currentTime">Current time UTC</param>
    private static (DateTime startTime, DateTime endTime) GetTimeIntervalToCheck(Visit visit, DateTime currentTime)
    {
        if (visit.HasStarted())
        {
            var maxVisitDurationMinutes = visit.Restaurant.MaxReservationDurationMinutes;
            return (currentTime, visit.StartTime!.Value.AddMinutes(maxVisitDurationMinutes));
        }

        if (visit.Reservation is not null)
        {
            return (visit.Reservation.StartTime, visit.Reservation.EndTime);
        }

        throw new InvalidOperationException(
            $"Visit has neither {nameof(Visit.StartTime)} nor {nameof(Visit.Reservation)} set");
    }

    /// <summary>
    /// Load a visit by ID including and its restaurant
    /// </summary>
    private async Task<Visit?> FindVisitIncludingRestaurant(int visitId)
    {
        return await context.Visits
            .Include(v => v.Restaurant)
            .FirstOrDefaultAsync(v => v.VisitId == visitId);
    }

    /// <summary>
    /// Load a table by ID
    /// </summary>
    private async Task<Table?> FindTableInRestaurant(int tableId, int restaurantId)
    {
        return await context.Tables
            .FirstOrDefaultAsync(t => t.TableId == tableId && t.RestaurantId == restaurantId);
    }

    /// <summary>
    /// Get visits that might collide
    /// </summary>
    private async Task<List<Visit>> GetPossibleCollidingVisits(Visit currentVisit, Table table)
    {
        return await context.Visits
            .Include(v => v.Reservation)
            .Include(v => v.Restaurant)
            .Where(v => v != currentVisit)
            .Where(v => v.RestaurantId == table.RestaurantId && v.TableId == table.TableId)
            .Where(v => v.EndTime == null) // Wizyta jeszcze się nie zakończyła
            .ToListAsync();
    }
}
