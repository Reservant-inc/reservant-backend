﻿using AutoMapper;
using Reservant.ErrorCodeDocs.Attributes;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Deliveries;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services;

/// <summary>
/// Service for deliveries
/// </summary>
public class DeliveryService(
    ApiDbContext context,
    ValidationService validationService,
    AuthorizationService authorizationService,
    IMapper mapper)
{
    /// <summary>
    /// Get information about a delivery
    /// </summary>
    /// <param name="deliveryId">ID of the delivery</param>
    /// <param name="userId">ID of the current user for permission checking</param>
    [ErrorCode(null, ErrorCodes.NotFound)]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyRestaurantBackdoorAccess))]
    public async Task<Result<DeliveryVM>> GetDeliveryById(int deliveryId, Guid userId)
    {


        var delivery = await context.Deliveries
            .Include(d => d.Ingredients)
            .FirstOrDefaultAsync(d => d.DeliveryId == deliveryId);

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

        return mapper.Map<DeliveryVM>(delivery);
    }

    /// <summary>
    /// Create a new delivery
    /// </summary>
    /// <param name="request">Information about the new delivery</param>
    /// <param name="userId">ID of the current user for permission checking</param>
    [ErrorCode(null, ErrorCodes.AccessDenied)]
    [ErrorCode(nameof(request.Ingredients), ErrorCodes.NotFound,
        "Some of the ingredients were not found")]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyRestaurantBackdoorAccess))]
    [ValidatorErrorCodes<Delivery>]
    public async Task<Result<DeliveryVM>> PostDelivery(CreateDeliveryRequest request, Guid userId)
    {
        var access = await authorizationService.VerifyRestaurantBackdoorAccess(request.RestaurantId, userId);
        if (access.IsError)
        {
            return access.Errors;
        }

        var delivery = new Delivery
        {
            OrderTime = DateTime.UtcNow,
            RestaurantId = request.RestaurantId,
        };

        var countRealIngredients = await context.Ingredients
            .CountAsync(i => request.Ingredients
                .Select(ri => ri.IngredientId)
                .Contains(i.IngredientId));
        if (countRealIngredients != request.Ingredients.Count)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.Ingredients),
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = "Some of the ingredients were not found",
            };
        }

        var ingredients = request.Ingredients
            .Select(i => new IngredientDelivery
            {
                IngredientId = i.IngredientId,
                AmountOrdered = i.AmountOrdered,
                AmountDelivered = i.AmountDelivered,
                ExpiryDate = i.ExpiryDate,
                StoreName = i.StoreName,
            })
            .ToList();

        delivery.Ingredients = ingredients;

        var res = await validationService.ValidateAsync(delivery, userId);
        if (!res.IsValid)
        {
            return res;
        }

        context.Deliveries.Add(delivery);
        await context.SaveChangesAsync();

        return mapper.Map<DeliveryVM>(delivery);
    }
}
