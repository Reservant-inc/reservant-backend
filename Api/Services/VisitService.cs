using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.Api.Dtos.Visits;
using Reservant.Api.Identity;
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
    AuthorizationService authorizationService,
    UserManager<User> userManager)
{
    /// <summary>
    /// Gets the visit with the provided ID
    /// </summary>
    /// <returns></returns>
    public async Task<Result<VisitVM>> GetVisitByIdAsync(int visitId, User user)
    {
        var visit = await context.Visits
            .Include(x => x.Participants)
            .Include(x => x.Restaurant)
            .Where(x => x.VisitId == visitId)
            .FirstOrDefaultAsync();
        if (visit == null)
        {
            return new ValidationFailure { PropertyName = null, ErrorCode = ErrorCodes.NotFound };
        }

        var isParticipant = visit.ClientId == user.Id || visit.Participants.Contains(user);

        var canView = isParticipant;
        if (!canView)
        {
            canView = !(await authorizationService.VerifyRestaurantHallAccess(visit.RestaurantId, user.Id)).IsError;
        }

        if (!canView)
        {
            canView = await userManager.IsInRoleAsync(user, Roles.CustomerSupportAgent);
        }

        if (!canView)
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
    public async Task<Result> ApproveVisitRequestAsync(int visitId, User currentUser)
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

        if (result.IsError)
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
        await notificationService.NotifyVisitApprovedDeclined(visitFound.ClientId, visitId);

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
    public async Task<Result> DeclineVisitRequestAsync(int visitId, User currentUser)
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

        if (result.IsError)
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
        await notificationService.NotifyVisitApprovedDeclined(visitFound.ClientId, visitId);

        return Result.Success;
    }

    /// <summary>
    /// Confirm a visit's start as an employee
    /// </summary>
    /// <param name="visitId">ID of the visit</param>
    /// <param name="currentUserId">ID of the current user for permission checking</param>
    [ErrorCode(null, ErrorCodes.NotFound, "Visit not found")]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyRestaurantHallAccess))]
    [ErrorCode(null, ErrorCodes.IncorrectVisitStatus,
        "Visit has not been approved by restaurant or has already started")]
    public async Task<Result> ConfirmStart(int visitId, Guid currentUserId)
    {
        var visit = await context.Visits.SingleOrDefaultAsync(visit => visit.VisitId == visitId);
        if (visit is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = "Visit not found",
            };
        }

        var authResult = await authorizationService.VerifyRestaurantHallAccess(visit.RestaurantId, currentUserId);
        if (authResult.IsError) return authResult;

        if (visit.Reservation?.CurrentStatus is not ReservationStatus.ApprovedByRestaurant)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.IncorrectVisitStatus,
                ErrorMessage = "Visit cannot be started because it was not approved by the restaurant",
            };
        }

        if (visit.StartTime is not null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.IncorrectVisitStatus,
                ErrorMessage = "Visit has already started",
            };
        }

        visit.StartTime = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return Result.Success;
    }

    /// <summary>
    /// Confirm a visit's start as an employee
    /// </summary>
    /// <param name="visitId">ID of the visit</param>
    /// <param name="currentUserId">ID of the current user for permission checking</param>
    [ErrorCode(null, ErrorCodes.NotFound, "Visit not found")]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyRestaurantHallAccess))]
    [ErrorCode(null, ErrorCodes.IncorrectVisitStatus,
        "Visit has not started yet or is already ended")]
    public async Task<Result> ConfirmEnd(int visitId, Guid currentUserId)
    {
        var visit = await context.Visits.SingleOrDefaultAsync(visit => visit.VisitId == visitId);
        if (visit is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = "Visit not found",
            };
        }

        var authResult = await authorizationService.VerifyRestaurantHallAccess(visit.RestaurantId, currentUserId);
        if (authResult.IsError) return authResult;

        if (visit.StartTime is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.IncorrectVisitStatus,
                ErrorMessage = "Visit has not started yet",
            };
        }

        if (visit.EndTime is not null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.IncorrectVisitStatus,
                ErrorMessage = "Visit has already ended",
            };
        }

        visit.EndTime = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return Result.Success;
    }
}
