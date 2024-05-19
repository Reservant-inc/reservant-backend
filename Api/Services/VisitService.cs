namespace Reservant.Api.Services;
using Reservant.Api.Models.Dtos.Visit;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models.Dtos.User;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Models;
using ValidationFailure = FluentValidation.Results.ValidationFailure;
using Reservant.Api.Validators;
using Reservant.Api.Validation;


/// <summary>
/// Service for managing visits
/// </summary>
public class VisitService(ApiDbContext dbContext,ValidationService validationService)
{
    /// <summary>
    /// Gets the visist oof provided id
    /// </summary>
    /// <param name="visitId"></param>
    /// <returns></returns>
    public async Task<Result<VisitVM>> GetVisitByIdAsync(int visitId, User user)
    {
        var visit = await dbContext.Visits
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
            Id = visit.Id,
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
}
