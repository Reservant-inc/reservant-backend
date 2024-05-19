using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Validation;
using System.Reflection.Metadata.Ecma335;
using FluentValidation.Results;
using Reservant.Api.Validators;
using Reservant.Api.Models.Dtos.OrderItem;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing orders
/// </summary>
public class OrderService(ApiDbContext context, ValidationService validationService)
{
    public async Task<Result<OrderVM>> UpdateOrderStatusAsync(int id, UpdateOrderStatusRequest request, User user)
    {
        //does order with id exist?
        var order = await context.Orders
            .Where(o => o.Id == id)
            .Include(o => o.Visit)
            .ThenInclude(o => o.RestaurantId)
            .FirstOrDefaultAsync();
        if (order is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = ErrorCodes.NotFound
            };
        }
        var restaurant = await context.Restaurants.FindAsync(order.Visit.RestaurantId);
        if (restaurant is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = ErrorCodes.NotFound
            };
        }

/*        var result = await validationService.ValidateAsync(order);
        if (!result.IsValid)
        {
            return result;
        }*/

        return new OrderVM
        {
            Cost = order.Cost,
            OrderId = order.Id,
            VisitId = order.VisitId,
            Status = order.Status,
            Items = (List<OrderItemVM>)order.OrderItems.Select(orderItem => new OrderItemVM
            {
                Amount = orderItem.Amount,
                Status = orderItem.Status,
                MenuItemId = orderItem.MenuItemId,
                Cost = orderItem.MenuItem.Price * orderItem.Amount
            })
        };
        //is it in the correct restaurant? request.Visit.RestaurantId == user.RestaurantId

        //update status of the order
    }
}
