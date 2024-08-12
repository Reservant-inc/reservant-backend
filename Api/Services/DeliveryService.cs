using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Delivery;
using Reservant.Api.Models.Dtos.Ingredient;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services;

/// <summary>
/// Service for deliveries
/// </summary>
public class DeliveryService(
    ApiDbContext context,
    ValidationService validationService)
{
    public async Task<Result<DeliveryVM>> GetDeliveryById(int deliveryId)
    {
        var delivery = await context.Deliveries
            .FirstOrDefaultAsync(d => d.Id == deliveryId);

        if (delivery == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound
            };
        }
        return new DeliveryVM
        {
            Id = delivery.Id,
            OrderTime = delivery.OrderTime,
            DeliveredTime = delivery.DeliveredTime,
            RestaurantId = delivery.RestaurantId,
            UserId = delivery.UserId,
            Ingredients = await context.Entry(delivery)
                            .Collection(d => d.Ingredients)
                            .Query()
                            .Select(i => new IngredientDeliveryVM
                            {
                                DeliveryId = i.DeliveryId,
                                IngredientId = i.IngredientId,
                                AmountOrdered = i.AmountOrdered,
                                AmountDelivered = i.AmountDelivered,
                                ExpiryDate = i.ExpiryDate,
                                StoreName = i.StoreName,
                            })
                            .ToListAsync()
        };
    }

    public async Task<Result<DeliveryVM>> PostDelivery(CreateDeliveryRequest request, string userId)
    {
        var delivery = new Delivery
        {
            OrderTime = DateTime.UtcNow,
            RestaurantId = request.RestaurantId,
        };

        //check if user currently works in restaurant from request
        var employmentQuery = await context.Employments
            .AnyAsync(e => e.EmployeeId == userId && e.DateUntil == null && e.RestaurantId == request.RestaurantId);

        if (!employmentQuery)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        var res = await validationService.ValidateAsync(delivery, userId);
        if (!res.IsValid)
        {
            return res;
        }

        context.Deliveries.Add(delivery);
        await context.SaveChangesAsync();

        return new DeliveryVM
        {
            Id = delivery.Id,
            OrderTime = delivery.OrderTime,
            DeliveredTime = delivery.DeliveredTime,
            RestaurantId = delivery.RestaurantId,
            UserId = delivery.UserId,
            Ingredients = await context.Entry(delivery)
                            .Collection(d => d.Ingredients)
                            .Query()
                            .Select(i => new IngredientDeliveryVM
                            {
                                DeliveryId = i.DeliveryId,
                                IngredientId = i.IngredientId,
                                AmountOrdered = i.AmountOrdered,
                                AmountDelivered = i.AmountDelivered,
                                ExpiryDate = i.ExpiryDate,
                                StoreName = i.StoreName,
                            })
                            .ToListAsync()
        };

    }
}
