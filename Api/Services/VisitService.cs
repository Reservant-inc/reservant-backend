using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.User;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Models.Dtos.Visit;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing visits
/// </summary>
public class VisitService(
    ApiDbContext context,
    UserManager<User> userManager,
    ValidationService validationService)
{
    /// <summary>
    /// Gets the visist oof provided id
    /// </summary>
    /// <param name="visitId"></param>
    /// <returns></returns>
    public async Task<Result<VisitVM>> GetVisitByIdAsync(int visitId, User user)
    {
        var visit = await context.Visits
        .Include(r => r.Participants!)
        .Include(r => r.Orders!)
            .ThenInclude(o=>o.OrderItems!)
                .ThenInclude(o1=>o1.MenuItem!)
            .Where(x => x.Id == visitId)
            .FirstOrDefaultAsync();


        if (visit == null)
        {
            return new ValidationFailure { PropertyName = null, ErrorCode = ErrorCodes.NotFound };
        }

        if (visit.ClientId != user.Id && !visit.Participants!.Contains(user))
        {
            return new ValidationFailure { PropertyName = null, ErrorCode = ErrorCodes.AccessDenied };
        }

        var result = new VisitVM
        {
            VisitId = visit.Id,
            Date = visit.Date,
            NumberOfGuests = visit.NumberOfGuests,
            PaymentTime = visit.PaymentTime,
            Deposit = visit.Deposit,
            ReservationDate = visit.ReservationDate,
            Tip = visit.Tip,
            Takeaway = visit.Takeaway,
            ClientId = visit.ClientId,
            RestaurantId = visit.RestaurantId,
            TableId = visit.TableId,
            Participants = visit.Participants!.Select(p => new UserSummaryVM
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName
            }).ToList(),
            Orders = visit.Orders!.Select(o => new OrderSummaryVM
            {
                OrderId = o.Id,
                VisitId = o.VisitId,
                Cost = o.Cost,
                Status = o.Status
            }).ToList()
        };

        return result;
    }

    public async Task<Result<VisitSummaryVM>> CreateVisitAsync(CreateVisitRequest request, User user)
    {

        var result = await validationService.ValidateAsync(request);
        if (!result.IsValid)
        {
            return result;
        }

        var participants = new List<User>();

        foreach(var userId in request.Participants)
        {
            var currentUser = await userManager.FindByIdAsync(userId);
            if (currentUser != null) participants.Add(currentUser);
        }

        var visit = new Visit()
        {
            Date = request.Date,
            NumberOfGuests = request.NumberOfGuests,
            ReservationDate = DateOnly.FromDateTime(DateTime.Now),
            Tip = request.Tip,
            Client = user,
            ClientId = user.Id,
            Takeaway = request.Takeaway,
            TableRestaurantId = request.RestaurantId,
            TableId = request.TableId,
            Participants = participants
        };

        result = await validationService.ValidateAsync(visit);
        if (!result.IsValid)
        {
            return result;
        }

        context.Add(visit);
        await context.SaveChangesAsync();

        return new VisitSummaryVM()
        {
            VisitId = visit.Id,
            ClientId = visit.ClientId,
            Date = visit.Date,
            Takeaway = visit.Takeaway,
            RestaurantId = visit.RestaurantId,
            NumberOfPeople = visit.NumberOfGuests
        };
    }
}
