using ErrorCodeDocs.Attributes;
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
    /// <summary>
    /// Get information about a delivery
    /// </summary>
    /// <param name="deliveryId">ID of the delivery</param>
    [ErrorCode(null, ErrorCodes.NotFound)]
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

    /// <summary>
    /// Create a new delivery
    /// </summary>
    /// <param name="request">Information about the new delivery</param>
    /// <param name="userId">ID of the current user for permission checking</param>
    [ErrorCode(null, ErrorCodes.AccessDenied)]
    [ValidatorErrorCodes<Delivery>]
    public async Task<Result<DeliveryVM>> PostDelivery(CreateDeliveryRequest request, string userId)
    {
        var delivery = new Delivery
        {
            OrderTime = DateTime.UtcNow,
            RestaurantId = request.RestaurantId,
        };

        var ingredients = request.Ingredients
            .Select(i => new IngredientDelivery
            {
                DeliveryId = i.DeliveryId,
                IngredientId = i.IngredientId,
                AmountOrdered = i.AmountOrdered,
                AmountDelivered = i.AmountDelivered,
                ExpiryDate = i.ExpiryDate,
                StoreName = i.StoreName,
            })
            .ToList();

        delivery.Ingredients = ingredients;

        //check if user currently works in restaurant from request
        var employmentQuery = await context.Employments
            .AnyAsync(e => e.EmployeeId == userId && e.DateUntil == null && e.RestaurantId == request.RestaurantId);

        if (!employmentQuery)
        {

            var ownerQuery = await context.Restaurants
                .AnyAsync(r => r.Id == request.RestaurantId && r.Group.OwnerId == userId);

            if (!ownerQuery)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

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
