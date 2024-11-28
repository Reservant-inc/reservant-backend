using AutoMapper;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Deliveries;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services.DeliveryServices;

/// <summary>
/// Service responsible for marking a delivery as canceled
/// </summary>
public class MarkCanceledService(
    ApiDbContext context,
    AuthorizationService authorizationService,
    IMapper mapper)
{
    /// <summary>
    /// Mark the delivery as canceled
    /// </summary>
    /// <param name="deliveryId">ID of the delivery</param>
    /// <param name="userId">ID of the current user</param>
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyRestaurantBackdoorAccess))]
    [ErrorCode(nameof(deliveryId), ErrorCodes.DeliveryNotPending,
        "Delivery has already been confirmed or canceled")]
    public async Task<Result<DeliveryVM>> MarkCanceled(int deliveryId, Guid userId)
    {
        var delivery = await context.Deliveries
            .Include(d => d.Ingredients)
            .ThenInclude(i => i.Ingredient)
            .SingleAsync(d => d.DeliveryId == deliveryId);

        var auth = await authorizationService.VerifyRestaurantBackdoorAccess(delivery.RestaurantId, userId);
        if (auth.IsError) return auth.Errors;

        if (!delivery.IsPending)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(deliveryId),
                ErrorCode = ErrorCodes.DeliveryNotPending,
                ErrorMessage = "Delivery has already been confirmed or canceled",
            };
        }

        delivery.DeliveredTime = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return mapper.Map<DeliveryVM>(delivery);
    }
}
