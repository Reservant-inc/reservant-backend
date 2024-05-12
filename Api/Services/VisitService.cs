using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
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
    ValidationService validationService
    )
{

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

        var visit = new Visit()
        {

        };
        
        return new VisitSummaryVM()
        {
            ClientId = user.Id,
            Date = request.Date,
            Takeaway = request.Takeaway,
            RestaurantId = request.RestaurantId
        };
    }
    
    
    
}
