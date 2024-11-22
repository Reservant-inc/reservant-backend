using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
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
        // Pobierz wizytę
        var visit = await context.Visits
            .Include(v => v.Reservation)
            .Include(v => v.Restaurant)
            .FirstOrDefaultAsync(v => v.VisitId == visitId);

        if (visit == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(visitId),
                ErrorMessage = "VIsit not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        var authResult = await authorizationService.VerifyRestaurantHallAccess(visit.RestaurantId, currentUserId);
        if (authResult.IsError) return authResult;

        var currentTime = DateTime.Now;
        if (visit.EndTime != null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Cannot change table for a visit that has already ended",
                ErrorCode = ErrorCodes.IncorrectVisitStatus
            };
        }

        var newTable = await context.Tables
            .FirstOrDefaultAsync(t => t.TableId == newTableId && t.RestaurantId == visit.RestaurantId);

        if (newTable == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(newTableId),
                ErrorMessage = "Table not found in selected restaurant",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        DateTime visitStartTime;
        DateTime visitEndTime;

        if (visit.StartTime != null)
        {
            // Wizyta już się rozpoczęła; użyjemy aktualnego czasu jako czasu rozpoczęcia
            visitStartTime = currentTime;
        }
        else
        {
            // Wizyta jeszcze się nie rozpoczęła; musi to być rezerwacja
            if (visit.Reservation != null)
            {
                // Użyjemy czasu rozpoczęcia rezerwacji
                visitStartTime = visit.Reservation.StartTime;
            }
            else
            {
                // Ta sytuacja nie powinna wystąpić
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "Invalid state of the visit: visit has not started and there is no reservation",
                    ErrorCode = ErrorCodes.InvalidState
                };
            }
        }

        if (visit.Reservation != null)
        {
            // Użyjemy czasu zakończenia rezerwacji
            visitEndTime = visit.Reservation.EndTime;
        }
        else
        {
            // Wizyta bez rezerwacji (gość z ulicy)
            // visitStartTime + MaxReservationDurationMinutes restauracji
            var maxVisitDurationMinutes = visit.Restaurant.MaxReservationDurationMinutes;
            visitEndTime = visitStartTime.AddMinutes(maxVisitDurationMinutes);
        }

        // Pobranie potencjalnie kolidujących wizyt
        var potentialOverlappingVisits = await context.Visits
            .Include(v => v.Reservation)
            .Include(v => v.Restaurant)
            .Where(v => v.VisitId != visitId)
            .Where(v => v.TableId == newTableId)
            .Where(v => v.EndTime == null) // Wizyta jeszcze się nie zakończyła
            .ToListAsync();

        // Sprawdzenie nakładających się wizyt
        bool overlappingVisits = potentialOverlappingVisits.Any(v =>
        {
            DateTime otherVisitStartTime;
            DateTime otherVisitEndTime;

            if (v.StartTime != null)
            {
                // Wizyta już się rozpoczęła
                // Używamy currentTime jako czasu rozpoczęcia dla sprawdzenia dostępności
                otherVisitStartTime = currentTime;

                // Wizyta jeszcze się nie zakończyła; obliczamy przewidywany czas zakończenia
                var maxDuration = v.Restaurant.MaxReservationDurationMinutes;
                otherVisitEndTime = v.StartTime.Value.AddMinutes(maxDuration);
            }
            else if (v.Reservation != null)
            {
                // Wizyta jeszcze się nie rozpoczęła, ale ma rezerwację
                otherVisitStartTime = v.Reservation.StartTime;
                otherVisitEndTime = v.Reservation.EndTime;
            }
            else
            {
                // Ta sytuacja nie powinna wystąpić według Twojego modelu
                return false;
            }

            // Sprawdzenie nakładania się przedziałów czasowych
            return (otherVisitStartTime < visitEndTime) && (otherVisitEndTime > visitStartTime);
        });

        if (overlappingVisits)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(newTableId),
                ErrorMessage = "The table is nov available in selected time period",
                ErrorCode = ErrorCodes.TableNotAvailable
            };
        }

        visit.TableId = newTableId;
        await context.SaveChangesAsync();

        return Result.Success;
    }
}
