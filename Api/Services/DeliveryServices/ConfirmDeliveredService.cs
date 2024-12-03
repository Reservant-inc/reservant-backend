using System.Globalization;
using AutoMapper;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Deliveries;
using Reservant.Api.Dtos.Ingredients;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services.DeliveryServices;

/// <summary>
/// Service responsible for confirming that a delivery has been delivered
/// </summary>
public class ConfirmDeliveredService(
    ApiDbContext context,
    AuthorizationService authorizationService,
    IMapper mapper,
    IngredientService ingredientService)
{
    /// <summary>
    /// Confirm that a delivery has been delivered and update the ingredient amounts
    /// </summary>
    /// <param name="deliveryId">ID of the delivery</param>
    /// <param name="userId">ID of the current user</param>
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyRestaurantBackdoorAccess))]
    [ErrorCode(nameof(deliveryId), ErrorCodes.NotFound)]
    [ErrorCode(nameof(deliveryId), ErrorCodes.DeliveryNotPending,
        "Delivery has already been confirmed or canceled")]
    public async Task<Result<DeliveryVM>> ConfirmDelivered(int deliveryId, Guid userId)
    {
        await context.Database.BeginTransactionAsync();

        var delivery = await context.Deliveries
            .Include(d => d.Ingredients)
            .ThenInclude(i => i.Ingredient)
            .FirstOrDefaultAsync(d => d.DeliveryId == deliveryId);

        if (delivery == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(deliveryId),
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = "Delivery not found",
            };
        }

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

        await UpdateIngredientAmounts(delivery, userId);
        await context.Database.CommitTransactionAsync();

        return mapper.Map<DeliveryVM>(delivery);
    }

    private async Task UpdateIngredientAmounts(Delivery delivery, Guid currentUserId)
    {
        var comment = GetCorrectionCommentForDelivery(delivery);
        foreach (var item in delivery.Ingredients)
        {
            (await ingredientService.CorrectIngredientAmountAsync(
                item.IngredientId,
                currentUserId,
                new IngredientAmountCorrectionRequest
                {
                    NewAmount = item.Ingredient.Amount + item.AmountOrdered,
                    Comment = comment,
                })).OrThrow();
        }
    }

    private static string GetCorrectionCommentForDelivery(Delivery delivery) =>
        $"Received delivery ID {delivery.DeliveryId} from {delivery.OrderTime.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)}";
}
