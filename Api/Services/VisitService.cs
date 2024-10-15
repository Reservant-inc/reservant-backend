using AutoMapper;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.Api.Dtos.Visits;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing visits
/// </summary>
public class VisitService(
    ApiDbContext context,
    UserManager<User> userManager,
    ValidationService validationService,
    IMapper mapper)
{
    /// <summary>
    /// Gets the visit with the provided ID
    /// </summary>
    /// <returns></returns>
    public async Task<Result<VisitVM>> GetVisitByIdAsync(int visitId, User user)
    {
        var visit = await context.Visits
        .Include(r => r.Participants)
        .Include(r => r.Orders)
            .ThenInclude(o=>o.OrderItems)
                .ThenInclude(o1=>o1.MenuItem)
            .Where(x => x.VisitId == visitId)
            .FirstOrDefaultAsync();


        if (visit == null)
        {
            return new ValidationFailure { PropertyName = null, ErrorCode = ErrorCodes.NotFound };
        }

        if (visit.ClientId != user.Id && !visit.Participants.Contains(user))
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

        var result = await validationService.ValidateAsync(request, user.Id);
        if (!result.IsValid)
        {
            return result;
        }

        var restaurant = await context.Restaurants
            .FirstAsync(r => r.RestaurantId == request.RestaurantId);

        var participants = new List<User>();

        foreach(var userId in request.ParticipantIds)
        {
            var currentUser = await userManager.FindByIdAsync(userId.ToString());
            if (currentUser != null) participants.Add(currentUser);
        }

        var visit = new Visit()
        {
            Date = request.Date,
            NumberOfGuests = request.NumberOfGuests,
            ReservationDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Tip = request.Tip,
            Client = user,
            ClientId = user.Id,
            Takeaway = request.Takeaway,
            RestaurantId = request.RestaurantId,
            TableId = request.TableId,
            Participants = participants,
            Deposit = restaurant.ReservationDeposit
        };

        result = await validationService.ValidateAsync(visit, user.Id);
        if (!result.IsValid)
        {
            return result;
        }

        context.Add(visit);
        await context.SaveChangesAsync();

        return mapper.Map<VisitSummaryVM>(visit);
    }
}
