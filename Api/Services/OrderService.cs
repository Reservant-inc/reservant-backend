using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Validation;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing orders
/// </summary>
public class OrderService(
        ValidationService validationService,
        ApiDbContext context
    ) 
{

    /// <summary>
    /// Creating new order
    /// </summary>
    public async Task<Result<OrderSummaryVM>> CreateOrderAsync(CreateOrderRequest request)
    {
        var menuItemIds = request.Items.Select(i => i.MenuItemId).ToList();
        var menuItems = await context.MenuItems
            .Where(mi => menuItemIds.Contains(mi.Id))
            .ToListAsync();
        
        var result = await validationService.ValidateAsync(request);
        
        if (!result.IsValid)
        {
            return result;
        }

        var orderItems = request.Items.Select(c => new OrderItem
        {
            MenuItemId = c.MenuItemId,
            Amount = c.Amount,
            Status = OrderStatus.InProgress,
            MenuItem = menuItems.First(mi => mi.Id == c.MenuItemId)
        }).ToList();

        var order = new Order
        {
            VisitId = request.VisitId,
            Note = request.Note,
            OrderItems = orderItems
        };

        result = await validationService.ValidateAsync(order);
        if (!result.IsValid)
        {
            return result;
        }

        context.Add(order);
        await context.SaveChangesAsync();


        return new OrderSummaryVM()
        {
            OrderId = order.Id,
            Cost = order.Cost,
            Note = order.Note,
            Status = order.Status,
            VisitId = order.VisitId
        };
    }
}