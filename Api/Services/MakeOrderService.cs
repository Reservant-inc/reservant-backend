using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Orders;
using Reservant.Api.Models.Enums;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.ErrorCodeDocs.Attributes;
using Reservant.Api.Validators;
using Microsoft.EntityFrameworkCore;
using FluentValidation.Results;

namespace Reservant.Api.Services;

/// <summary>
/// Service used for making new orders
/// </summary>
/// <param name="context"></param>
/// <param name="validationService"></param>
/// <param name="paymentService"></param>
/// <param name="authorizationService"></param>
/// <param name="mapper"></param>
public class MakeOrderService(ApiDbContext context, ValidationService validationService, PaymentService paymentService, AuthorizationService authorizationService, IMapper mapper)
{
    /// <summary>
    /// Creating new order
    /// </summary>
    [ErrorCode(nameof(request.Items), ErrorCodes.NotFound,
        "Some of the menu items were not found")]
    [ErrorCode(nameof(request.Items), ErrorCodes.BelongsToAnotherRestaurant,
        "Order must only include items from the visit's restaurant")]
    [ErrorCode(nameof(request.Items), ErrorCodes.NotInAMenu,
        "Order must only include items that are included in an active menu")]
    [MethodErrorCodes<AuthorizationService>(nameof(AuthorizationService.VerifyVisitParticipant))]
    [ValidatorErrorCodes<CreateOrderRequest>]
    [ValidatorErrorCodes<Order>]
    [MethodErrorCodes<PaymentService>(nameof(PaymentService.PayForOrderAsync))]
    public async Task<Result<OrderSummaryVM>> CreateOrderAsync(CreateOrderRequest request, User user)
    {
        var todaysDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await validationService.ValidateAsync(request, user.Id);
        if (!result.IsValid)
        {
            return result;
        }

        var menuItemIds = request.Items.Select(i => i.MenuItemId).ToList();
        var menuItems = await context.MenuItems
            .Where(mi => menuItemIds.Contains(mi.MenuItemId))
            .ToDictionaryAsync(mi => mi.MenuItemId);

        if (menuItems.Count != request.Items.Count)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.Items),
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = "Some of the menu items were not found",
            };
        }

        var access = await authorizationService
            .VerifyVisitParticipant(request.VisitId, user.Id);
        if (access.IsError)
        {
            return access.Errors;
        }

        var visit = await context.Visits
            .Where(v => v.VisitId == request.VisitId)
            .Include(visit => visit.Participants)
            .Include(v => v.Restaurant)
            .Include(v => v.Reservation)
            .FirstAsync();

        if (menuItems.Values.Any(mi => mi.RestaurantId != visit.RestaurantId))
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.Items),
                ErrorCode = ErrorCodes.BelongsToAnotherRestaurant,
                ErrorMessage = "Order must only include items from the visit's restaurant",
            };
        }

        var availableMenuItemCount = await context.MenuItems
            .Where(mi => menuItemIds.Contains(mi.MenuItemId))
            .CountAsync(mi => mi.Menus.Any(m => m.DateUntil == null || m.DateUntil >= todaysDate));

        if (availableMenuItemCount != menuItems.Count)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.Items),
                ErrorCode = ErrorCodes.NotInAMenu,
                ErrorMessage = "All menuItems must be in a publicly available menu",
            };
        }

        var orderItems = request.Items.Select(c => new OrderItem
        {
            MenuItemId = c.MenuItemId,
            Amount = c.Amount,
            OneItemPrice = menuItems[c.MenuItemId].Price,
            Status = OrderStatus.InProgress,
        }).ToList();

        var order = new Order
        {
            VisitId = request.VisitId,
            Note = request.Note,
            OrderItems = orderItems,
            Visit = visit
        };

        result = await validationService.ValidateAsync(order, user.Id);
        if (!result.IsValid)
        {
            return result;
        }

        context.Add(order);
        
        if (visit.StartTime == null) {
            var transaction = await paymentService.PayForOrderAsync(user, order);
            if (transaction.IsError) {
                return transaction.Errors;
            }
        }

        await context.SaveChangesAsync();

        return mapper.Map<OrderSummaryVM>(order);
    }
}
