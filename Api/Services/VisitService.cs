using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.Api.Dtos.Visits;
using Reservant.Api.Dtos.Orders;
using Reservant.Api.Dtos.Users;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing visits
/// </summary>
public class VisitService(
    ApiDbContext context,
    UserManager<User> userManager,
    ValidationService validationService,
    FileUploadService uploadService)
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

        var result = new VisitVM
        {
            VisitId = visit.VisitId,
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
            Participants = visit.Participants.Select(p => new UserSummaryVM
            {
                UserId = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Photo = uploadService.GetPathForFileName(p.PhotoFileName),
            }).ToList(),
            Orders = visit.Orders.Select(o => new OrderSummaryVM
            {
                OrderId = o.OrderId,
                VisitId = o.VisitId,
                Date = o.Visit.Date,
                Cost = o.OrderItems.Sum(oi => oi.Price * oi.Amount),
                Status = o.OrderItems.Select(oi => oi.Status).MaxBy(s => (int)s)
            }).ToList()
        };

        return result;
    }

    /// <summary>
    /// Create a Visit
    /// </summary>
    /// <param name="request">Description of the new visit</param>
    /// <param name="user">Owner of the visit</param>
    /// <returns></returns>
    public async Task<Result<VisitSummaryVM>> CreateVisitAsync(CreateVisitRequest request, User user)
    {
        // Validate the request
        var validationResult = await validationService.ValidateAsync(request, user.Id);
        if (!validationResult.IsValid)
        {
            return validationResult;
        }

        // Fetch restaurant information
        var restaurant = await context.Restaurants
            .Include(r => r.Tables)
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

        // £¹czna liczba ludzi to liczba goœci którzy nie maj¹ konta + liczba goœci któzy maj¹ konto i je podaliœmy + osoba sk³¹daj¹ca zamówienie
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

        return new VisitSummaryVM
        {
            VisitId = visit.VisitId,
            ClientId = visit.ClientId,
            Date = visit.Date,
            EndTime = visit.EndTime,
            Takeaway = visit.Takeaway,
            RestaurantId = visit.RestaurantId,
            NumberOfPeople = visit.NumberOfGuests + visit.Participants.Count + 1,
            Deposit = visit.Deposit
        };
    }


}
