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
    ValidationService validationService,
    AuthorizationService authorizationService)
{
    /// <summary>
    /// Get information about a delivery
    /// </summary>
    /// <param name="deliveryId">ID of the delivery</param>
    /// <param name="userId">ID of the current user for permission checking</param>
    [ErrorCode(null, ErrorCodes.NotFound)]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyRestaurantBackdoorAccess))]
    public async Task<Result<DeliveryVM>> GetDeliveryById(int deliveryId, string userId)
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

        var access = await authorizationService.VerifyRestaurantBackdoorAccess(delivery.RestaurantId, userId);
        if (access.IsError)
        {
            return access.Errors;
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
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyRestaurantBackdoorAccess))]
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

        var access = await authorizationService.VerifyRestaurantBackdoorAccess(delivery.RestaurantId, userId);
        if (access.IsError)
        {
            return access.Errors;
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
