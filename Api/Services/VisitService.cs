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
using Reservant.Api.Models.Enums;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing visits
/// </summary>
public class VisitService(
    ApiDbContext context,
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
    /// Reject a visit request as resturant owner or employee
    /// </summary>
    /// <param name="visitId">ID of the event</param>
    /// <param name="currentUser">Current user for permission checking</param>
    [ErrorCode(nameof(visitId), ErrorCodes.NotFound, "Visit not found")]
    [ErrorCode(nameof(visitId), ErrorCodes.IncorrectVisitStatus, "Reservation already reviewed or deposit not paid")]
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

        if (visitFound.Reservation?.CurrentStatus is not ReservationStatus.ToBeReviewedByRestaurant)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(visitId),
                ErrorMessage = "Reservation already reviewed or deposit not paid",
                ErrorCode = ErrorCodes.IncorrectVisitStatus,
            };
        }

        visitFound.Reservation.Decision = new RestaurantDecision
        {
            AnsweredBy = currentUser,
            IsAccepted = true,
        };

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
    [ErrorCode(nameof(visitId), ErrorCodes.IncorrectVisitStatus, "Reservation already reviewed or deposit not paid")]
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

        if (visitFound.Reservation?.CurrentStatus is not ReservationStatus.ToBeReviewedByRestaurant)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(visitId),
                ErrorMessage = "Reservation already reviewed or deposit not paid",
                ErrorCode = ErrorCodes.IncorrectVisitStatus,
            };
        }

        visitFound.Reservation.Decision = new RestaurantDecision
        {
            AnsweredBy = currentUser,
            IsAccepted = false,
        };

        await context.SaveChangesAsync();
        await notificationService.NotifyVisitApprovedDeclined(visitFound.ClientId,visitId);

        return Result.Success;
    }
}
