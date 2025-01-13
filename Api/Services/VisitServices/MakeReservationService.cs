using AutoMapper;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Restaurants;
using Reservant.Api.Dtos.Visits;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services.VisitServices;

/// <summary>
/// Service responsible for creating a reservation in the future
/// </summary>
public class MakeReservationService(
    ApiDbContext context,
    ValidationService validationService,
    IMapper mapper,
    NotificationService notificationService,
    AuthorizationService authorizationService)
{
    /// <summary>
    /// Make a reservation at a restaurant
    /// </summary>
    /// <param name="request">Description of the reservation</param>
    /// <param name="client">Client that is making the reservation</param>
    [ValidatorErrorCodes<MakeReservationRequest>]
    [ErrorCode(nameof(request.RestaurantId), ErrorCodes.NotFound)]
    [MethodErrorCodes<MakeReservationService>(nameof(CheckReservationDuration))]
    [ErrorCode(null, ErrorCodes.Duplicate,
        "You already have a reservation during this time period.")]
    [ErrorCode(null, ErrorCodes.NoAvailableTable)]
    [ValidatorErrorCodes<Visit>]
    public async Task<Result<VisitSummaryVM>> MakeReservation(MakeReservationRequest request, User client)
    {
        var requestValidation = await validationService.ValidateAsync(request, client.Id);
        if (!requestValidation.IsValid) return requestValidation;

        var restaurant = await GetRestaurant(request.RestaurantId);
        if (restaurant is null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.RestaurantId),
                ErrorMessage = "Restaurant not found",
                ErrorCode = ErrorCodes.NotFound,
            };
        }

        var requestedTimeIsValid = CheckReservationDuration(request, restaurant);
        if (requestedTimeIsValid.IsError) return requestedTimeIsValid.Errors;

        if (await ClientHasReservation(client, from: request.Date, until: request.EndTime))
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "You already have a reservation during this time period.",
                ErrorCode = ErrorCodes.Duplicate,
            };
        }

        var participants = await FindParticipantsByIds(request.ParticipantIds);
        var visit = new Visit
        {
            Restaurant = restaurant,
            NumberOfGuests = request.NumberOfGuests,
            Creator = client,
            Participants = participants,
            Reservation = new Reservation
            {
                StartTime = request.Date,
                EndTime = request.EndTime,
                ReservationDate = DateTime.UtcNow,
                Deposit = restaurant.ReservationDeposit,
            },
            Tip = request.Tip,
            Takeaway = request.Takeaway,
        };

        if (!request.Takeaway)
        {
            var table = await FindSmallestAvailableTableFor(
            request.TotalNumberOfPeople, restaurant,
            from: request.Date, until: request.EndTime);
            if (table is null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "No available tables for the requested time slot",
                    ErrorCode = ErrorCodes.NoAvailableTable,
                };
            }

            visit.TableId = table.TableId;

        }

        context.Add(visit);
        var validationResult = await validationService.ValidateAsync(visit, client.Id);
        if (!validationResult.IsValid) return validationResult;
        await context.SaveChangesAsync();

        await NotifyHallEmployeesAboutReservation(restaurant, visit);
        return mapper.Map<VisitSummaryVM>(visit);
    }

    /// <summary>
    /// Check if the reservation duration is valid
    /// </summary>
    [ErrorCode(nameof(request.EndTime), ErrorCodes.VisitExceedsMaxTime)]
    [ErrorCode(nameof(request.EndTime), ErrorCodes.VisitTooShort)]
    private static Result CheckReservationDuration(MakeReservationRequest request, Restaurant restaurant)
    {
        var durationMinutes = request.EndTime.Subtract(request.Date).TotalMinutes;

        if (durationMinutes > restaurant.MaxReservationDurationMinutes)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.EndTime),
                ErrorMessage = "Visit duration exceeds restaurant maximum visit time.",
                ErrorCode = ErrorCodes.VisitExceedsMaxTime,
            };
        }

        if (durationMinutes < Visit.MinReservationDurationMinutes)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.EndTime),
                ErrorMessage = $"Visit duration must be at least {Visit.MinReservationDurationMinutes}min.",
                ErrorCode = ErrorCodes.VisitTooShort,
            };
        }

        return Result.Success;
    }

    /// <summary>
    /// Find requested participants for the visit by IDs
    /// </summary>
    private async Task<List<User>> FindParticipantsByIds(List<Guid> participantIds)
    {
        return await context.Users
            .Where(u => participantIds.Contains(u.Id))
            .ToListAsync();
    }

    /// <summary>
    /// Find the smallest available table
    /// </summary>
    /// <param name="restaurant">Restaurant in which to find the table</param>
    /// <param name="numberOfPeople">Number of people trying to make a reservation</param>
    /// <param name="from">Starting time of the reservation</param>
    /// <param name="until">Ending time of the reservation</param>
    /// <returns></returns>
    private async Task<Table?> FindSmallestAvailableTableFor(
        int numberOfPeople,
        Restaurant restaurant,
        DateTime from,
        DateTime until)
    {
        return await context.Tables
            .Where(t => t.RestaurantId == restaurant.RestaurantId && t.Capacity >= numberOfPeople)
            .Where(t => !context.Visits.Any(v =>
                v.TableId == t.TableId &&
                v.Reservation!.StartTime < until &&
                v.Reservation!.EndTime > from))
            .OrderBy(t => t.Capacity)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Notify hall employees of a restaurant that there is a new reservation
    /// </summary>
    private async Task NotifyHallEmployeesAboutReservation(Restaurant restaurant, Visit reservation)
    {
        var hallEmployees = await context.Entry(restaurant)
            .Collection(r => r.Employments)
            .Query()
            .Where(e => e.DateUntil == null && e.IsHallEmployee)
            .Select(e => e.EmployeeId)
            .ToListAsync();
        await notificationService.NotifyNewReservation(hallEmployees, reservation);
    }

    /// <summary>
    /// Get restaurant by the given ID
    /// </summary>
    private async Task<Restaurant?> GetRestaurant(int restaurantId)
    {
        return await context.Restaurants
            .OnlyActiveRestaurants()
            .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId);
    }

    /// <summary>
    /// Check if there is a reservation by a given client at a given time span
    /// </summary>
    private async Task<bool> ClientHasReservation(User client, DateTime from, DateTime until)
    {
        return await context.Visits
            .AnyAsync(v => v.CreatorId == client.Id
                           && v.Reservation!.StartTime < until && v.Reservation!.EndTime > from);
    }

    /// <summary>
    /// Make a reservation at a restaurant for a Client that doesn't use our application
    /// </summary>
    /// <param name="request">Description of the reservation</param>
    /// <param name="emp">Employee that is making the reservation for a Client</param>
    [ValidatorErrorCodes<MakeReservationRequest>]
    [ErrorCode(nameof(request.RestaurantId), ErrorCodes.NotFound)]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyRestaurantHallAccess))]
    [MethodErrorCodes<MakeReservationService>(nameof(CheckReservationDuration))]
    [ErrorCode(null, ErrorCodes.Duplicate,
        "You already have a reservation during this time period.")]
    [ErrorCode(null, ErrorCodes.NoAvailableTable)]
    [ValidatorErrorCodes<Visit>]
    public async Task<Result<VisitSummaryVM>> MakeVisitEmployee(MakeReservationRequest request, User emp)
    {
        var requestValidation = await validationService.ValidateAsync(request, emp.Id);
        if (!requestValidation.IsValid) return requestValidation;

        var restaurant = await GetRestaurant(request.RestaurantId);
        if (restaurant is null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.RestaurantId),
                ErrorMessage = "Restaurant not found",
                ErrorCode = ErrorCodes.NotFound,
            };
        }

        var isHallEmployee = await authorizationService.VerifyRestaurantHallAccess(restaurant.RestaurantId, emp.Id);
        if (isHallEmployee.IsError) return isHallEmployee.Errors;

        var requestedTimeIsValid = CheckReservationDuration(request, restaurant);
        if (requestedTimeIsValid.IsError) return requestedTimeIsValid.Errors;

        var participants = await FindParticipantsByIds(request.ParticipantIds);
        var visit = new Visit
        {
            Restaurant = restaurant,
            NumberOfGuests = request.NumberOfGuests,
            Creator = emp,
            Participants = participants,
            Reservation = new Reservation
            {
                StartTime = request.Date,
                EndTime = request.EndTime,
                ReservationDate = DateTime.UtcNow,
                Deposit = restaurant.ReservationDeposit,
                DepositPaymentTime = request.Date,
                Decision = new RestaurantDecision
                {
                    AnsweredBy = emp,
                    IsAccepted = true,
                },
            },
            Tip = request.Tip,
            Takeaway = request.Takeaway,
            CreatedByEmployee = true,
        };

        if (!request.Takeaway)
        {
            var table = await FindSmallestAvailableTableFor(
            request.TotalNumberOfPeople, restaurant,
            from: request.Date, until: request.EndTime);
            if (table is null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "No available tables for the requested time slot",
                    ErrorCode = ErrorCodes.NoAvailableTable,
                };
            }

            visit.TableId = table.TableId;

        }

        context.Add(visit);
        var validationResult = await validationService.ValidateAsync(visit, emp.Id);
        if (!validationResult.IsValid) return validationResult;
        await context.SaveChangesAsync();

        return mapper.Map<VisitSummaryVM>(visit);
    }

}
