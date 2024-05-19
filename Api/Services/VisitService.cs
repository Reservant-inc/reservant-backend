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
            Id = visit.Id,
            ClientId = visit.ClientId,
            Date = visit.Date,
            Takeaway = visit.Takeaway,
            RestaurantId = visit.RestaurantId,
            NumberOfPeople = visit.NumberOfGuests
        };
    }
    
    
    
}
