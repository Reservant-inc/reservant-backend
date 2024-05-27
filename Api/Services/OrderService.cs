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
using Reservant.Api.Models.Dtos.MenuItem;
using System.Linq;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing orders
/// </summary>
public class OrderService(ApiDbContext context, ValidationService validationService)
{
    /// <summary>
    /// Update status of the order by updating the list of states of included menu items
    /// </summary>
    /// <param name="id">order id</param>
    /// <param name="request">request containing list of updated menu items and employees that work on them</param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Result<OrderVM>> UpdateOrderStatusAsync(int id, UpdateOrderStatusRequest request, User user)
    {
        var order = await context.Orders
            .Where(o => o.Id == id)
            .Include(o => o.Employees)
            .Include(o => o.OrderItems)
            .Include(o => o.Visit)
            .ThenInclude(v => v.Restaurant)
            .ThenInclude(r => r.Group)
            .FirstOrDefaultAsync();
        //does order with id exist?
        if (order is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = "Order not found"
            };
        }
        if (order.Visit.Restaurant is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = $"Restaurant not found, visit: visitID - {order.VisitId}, restaurantID - {order.Visit.Restaurant?.Id}"
            };
        }

        //is it in the correct restaurant group?
        if (order.Visit.Restaurant.Group.OwnerId != user.EmployerId)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.AccessDenied,
                ErrorMessage = ErrorCodes.AccessDenied
            };
        }

        //check if order items are in the order
        var itemIdsDict = new Dictionary<int, int>();
        for (int i = 0; i < order.OrderItems.Count; i++)
        {
            itemIdsDict.Add(order.OrderItems.ElementAt(i).MenuItemId, i);
        }

        //update status of the order's menuitems

        foreach (UpdateOrderItemStatusRequest item in request.Items)
        {
            if (itemIdsDict.ContainsKey(item.MenuItemId))
            {
                int itemIndex;
                itemIdsDict.TryGetValue(item.MenuItemId, out itemIndex);
                order.OrderItems.ElementAt(itemIndex).Status = item.Status;
            }
            else
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(request.Items),
                    ErrorCode = ErrorCodes.NotFound,
                    ErrorMessage = "Menu item not found in the order"
                };
            }
        }
        //assign employees to the order and check if they exist/belong to the correct restaurant group
        for (int i = 0; i < request.EmployeeIds.Count; i++)
        {
            var employee = await context.Users.FindAsync(request.EmployeeIds.ElementAt(i));
            if (employee is null)
            {
                return new ValidationFailure
                {
                    PropertyName = request.EmployeeIds.ElementAt(i),
                    ErrorCode = ErrorCodes.NotFound,
                    ErrorMessage = "Employee not found"
                };
            }
            if (employee.EmployerId != user.EmployerId)
            {
                return new ValidationFailure
                {
                    PropertyName = request.EmployeeIds.ElementAt(i),
                    ErrorCode = ErrorCodes.MustBeRestaurantEmployee,
                    ErrorMessage = ErrorCodes.MustBeRestaurantEmployee
                };
            }
            if (!(order.Employees.Contains(employee)))
            {
                order.Employees.Add(employee);
            }
        }
        await context.SaveChangesAsync();

        return new OrderVM
        {
            OrderId = order.Id,
            VisitId = order.VisitId,
            Status = order.Status,
            Items = order.OrderItems.Select(orderItem => new OrderItemVM
            {
                Amount = orderItem.Amount,
                Status = orderItem.Status,
                MenuItemId = orderItem.MenuItemId,
                Cost = context.MenuItems.Find(orderItem.MenuItemId).Price * orderItem.Amount
            }).ToList(),
            Cost = order.Cost,
            EmployeeId = order.EmployeeId
        };
    }
}
