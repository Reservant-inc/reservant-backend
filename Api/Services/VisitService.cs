using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using Reservant.Api.Data;
using Reservant.Api.Models;
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
    ValidationService validationService
    )
{

    public async Task<Result<List<VisitSummaryVM>>> GetVisitsAsync(User user)
    {
        return await context.Visits
            .Where(v => v.ClientId == user.Id)
            .Select(v => new VisitSummaryVM
            {
                ClientId = user.Id,
                Date = v.Date,
                Takeaway = v.Takeaway,
                RestaurantId = v.RestaurantId,
                NumberOfPeople = v.NumberOfGuests
            })
            .ToListAsync();
    }

    public async Task<Result<VisitSummaryVM>> CreateVisitAsync(CreateVisitRequest request, User user)
    {
        var restaurant = await context.Restaurants.FindAsync(request.RestaurantId);

        if (restaurant is null)
        {
            return new ValidationFailure()
            {
                PropertyName = nameof(request.RestaurantId),
                ErrorCode = $"Restaurant with ID {request.RestaurantId} not found.",
                ErrorMessage = ErrorCodes.NotFound
            };
        }
        
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
            ClientId = user.Id,
            Date = request.Date,
            Takeaway = request.Takeaway,
            RestaurantId = request.RestaurantId,
            NumberOfPeople = request.NumberOfGuests
        };
    }
    
    
    
}
