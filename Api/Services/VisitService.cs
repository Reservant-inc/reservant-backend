using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Restaurants;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.Api.Dtos.Visits;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing visits
/// </summary>
public class VisitService(
    ApiDbContext context,
    UserManager<User> userManager,
    ValidationService validationService,
    IMapper mapper,
    NotificationService notificationService,
    AuthorizationService authorizationService)
{
    /// <summary>
    /// Gets the visit with the provided ID
    /// </summary>
    /// <returns></returns>
    public async Task<Result<VisitVM>> GetVisitByIdAsync(int visitId, User user)
    {
        var visit = await context.Visits
            .Where(x => x.VisitId == visitId)
            .ProjectTo<VisitVM>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (visit == null)
        {
            return new ValidationFailure { PropertyName = null, ErrorCode = ErrorCodes.NotFound };
        }

        if (visit.ClientId != user.Id && visit.Participants.All(participant => participant.UserId != user.Id))
        {
            return new ValidationFailure { PropertyName = null, ErrorCode = ErrorCodes.AccessDenied };
        }

        return mapper.Map<VisitVM>(visit);
    }

    /// <summary>
    /// Create a Visit
    /// </summary>
    /// <param name="request">Description of the new visit</param>
    /// <param name="user">Owner of the visit</param>
    /// <returns></returns>
    public async Task<Result<VisitSummaryVM>> CreateVisitAsync(CreateVisitRequest request, User user)
    {
        // Walidacja czy godziny rezerwacji s� na pe�ne godziny lub p�godzinne
        if (request.Date.Minute != 0 && request.Date.Minute != 30)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.Date),
                ErrorMessage = "Reservations can only be made for full hours or half hours",
                ErrorCode = ErrorCodes.InvalidTimeSlot
            };
        }

        if (request.EndTime.Minute != 0 && request.EndTime.Minute != 30)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.EndTime),
                ErrorMessage = "Reservations can only be made for full hours or half hours",
                ErrorCode = ErrorCodes.InvalidTimeSlot
            };
        }

        // Validate the request
        var validationResult = await validationService.ValidateAsync(request, user.Id);
        if (!validationResult.IsValid)
        {
            return validationResult;
        }

        // Fetch restaurant information
        var restaurant = await context.Restaurants
            .Include(r => r.Tables)
            .OnlyActiveRestaurants()
            .FirstOrDefaultAsync(r => r.RestaurantId == request.RestaurantId);

        if (restaurant == null)
        {
            return new ValidationFailure
            {
                PropertyName = "RestaurantId",
                ErrorMessage = "Restaurant not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        // Check if the user already has a reservation during the requested time slot
        var overlappingReservation = await context.Visits
            .Where(v => v.ClientId == user.Id &&
                        v.Date < request.EndTime &&
                        v.EndTime > request.Date)
            .FirstOrDefaultAsync();

        if (overlappingReservation != null)
        {
            return new ValidationFailure
            {
                PropertyName = "ClientId",
                ErrorMessage = "You already have a reservation during this time period.",
                ErrorCode = ErrorCodes.Duplicate
            };
        }

        TimeSpan visitDuration = request.EndTime.Subtract(request.Date);

        if (visitDuration.TotalMinutes > restaurant.MaxReservationDuration)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.EndTime),
                ErrorMessage = "Visit duration exceeds restaurant maximum visit time.",
                ErrorCode = ErrorCodes.VisitExceedsMaxTime
            };
        }

        // ��czna liczba ludzi to liczba go�ci kt�rzy nie maj� konta + liczba go�ci kt�rzy maj� konto i je podali�my + osoba sk�adaj�ca zam�wienie
        var numberOfPeople = request.NumberOfGuests + request.ParticipantIds.Count + 1;

        // Find the smallest table that can accommodate the guests and is not reserved during the time slot
        var availableTable = await context.Tables
            .Where(t => t.RestaurantId == request.RestaurantId && t.Capacity >= numberOfPeople)
            .Where(t => !context.Visits.Any(v =>
                v.TableId == t.TableId &&
                v.Date < request.EndTime &&
                v.EndTime > request.Date))
            .OrderBy(t => t.Capacity)
            .FirstOrDefaultAsync();

        if (availableTable == null)
        {
            return new ValidationFailure
            {
                PropertyName = "TableId",
                ErrorMessage = "No available tables for the requested time slot",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        // Find the participants
        var participants = new List<User>();
        foreach (var userId in request.ParticipantIds)
        {
            var currentUser = await userManager.FindByIdAsync(userId.ToString());
            if (currentUser != null) participants.Add(currentUser);
        }

        // Create a new visit with end time
        var visit = new Visit
        {
            Date = request.Date,
            EndTime = request.EndTime,
            NumberOfGuests = request.NumberOfGuests,
            ReservationDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Tip = request.Tip,
            Client = user,
            ClientId = user.Id,
            Takeaway = request.Takeaway,
            RestaurantId = request.RestaurantId,
            TableId = availableTable.TableId,
            Participants = participants,
            Deposit = restaurant.ReservationDeposit
        };

        // Validate the visit object
        validationResult = await validationService.ValidateAsync(visit, user.Id);
        if (!validationResult.IsValid)
        {
            return validationResult;
        }

        // Save the visit
        context.Add(visit);
        await context.SaveChangesAsync();

        return mapper.Map<VisitSummaryVM>(visit);
    }

    /// <summary>
    /// Reject a visit request as resturant owner or employee
    /// </summary>
    /// <param name="visitId">ID of the event</param>
    /// <param name="currentUser">Current user for permission checking</param>
    [ErrorCode(nameof(visitId), ErrorCodes.NotFound, "Visit not found")]
    [ErrorCode(nameof(visitId), ErrorCodes.AlreadyConsidered, "User already considered")]
    [ErrorCode(null, ErrorCodes.AccessDenied, "User not qualified to reject")]
    public async Task<Result> ApproveVisitRequestAsync(int visitId,User currentUser)
    {
        var visitFound = await context.Visits
            .FirstOrDefaultAsync(e => e.VisitId == visitId);

        if (visitFound == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(visitId),
                ErrorMessage = "Event not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        var result = await authorizationService.VerifyRestaurantHallAccess(visitFound.RestaurantId, currentUser.Id);

        if(result.IsError)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "User not qualified to reject",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        if(visitFound.IsAccepted!=null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(visitId),
                ErrorMessage = "User already considered",
                ErrorCode = ErrorCodes.AlreadyConsidered
            };
        }

        visitFound.AnsweredBy = currentUser;
        visitFound.IsAccepted = true;

        await context.SaveChangesAsync();
        await notificationService.NotifyVisitApprovedDeclined(visitFound.ClientId,visitId);

        return Result.Success;
    }

    /// <summary>
    /// Reject a visit request as resturant owner or employee
    /// </summary>
    /// <param name="visitId">ID of the event</param>
    /// <param name="currentUser">Current user for permission checking</param>
    [ErrorCode(nameof(visitId), ErrorCodes.NotFound, "Visit not found")]
    [ErrorCode(nameof(visitId), ErrorCodes.AlreadyConsidered, "User already considered")]
    [ErrorCode(null, ErrorCodes.AccessDenied, "User not qualified to reject")]
    public async Task<Result> DeclineVisitRequestAsync(int visitId,User currentUser)
    {
        var visitFound = await context.Visits
            .FirstOrDefaultAsync(e => e.VisitId == visitId);

        if (visitFound == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(visitId),
                ErrorMessage = "Event not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        var result = await authorizationService.VerifyRestaurantHallAccess(visitFound.RestaurantId, currentUser.Id);

        if(result.IsError)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "User not qualified to reject",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        if(visitFound.IsAccepted!=null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(visitId),
                ErrorMessage = "User already considered",
                ErrorCode = ErrorCodes.AlreadyConsidered
            };
        }

        visitFound.AnsweredBy = currentUser;
        visitFound.IsAccepted = false;

        await context.SaveChangesAsync();
        await notificationService.NotifyVisitApprovedDeclined(visitFound.ClientId,visitId);

        return Result.Success;
    }
}
