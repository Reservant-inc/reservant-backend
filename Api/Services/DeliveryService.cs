using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
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
    UserManager<User> userManager,
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
}
