using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Models.Dtos.OrderItem;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing orders
/// </summary>
public class OrderService(
    UserManager<User> userManager,
    ApiDbContext context
    )
{
    /// <summary>
    /// Gets the order with the given id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Result<OrderVM>> GetOrderById(int id, ClaimsPrincipal claim)
    {

        var order = await context.Orders
            .Include(o => o.OrderItems!)
            .ThenInclude(oi => oi.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound
            };
        }

        var visit = await context.Visits
            .Include(v => v.Participants)
            .FirstOrDefaultAsync(v => v.Orders!.Contains(order));

        var user = await userManager.GetUserAsync(claim);
        
        var roles = await userManager.GetRolesAsync(user!);

        if (roles.Contains(Roles.Customer)
            && visit!.ClientId != user!.Id
            && !visit.Participants!.Contains(user))
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        if (roles.Contains(Roles.RestaurantEmployee))
        {
            var employment = await context.Employments
                .FirstOrDefaultAsync(e => e.EmployeeId == user!.Id);

            if (employment == null || visit!.RestaurantId != employment.RestaurantId)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }
            
        }

        var orderItems = order.OrderItems?.Select(i => new OrderItemVM()
        {
            MenuItemId = i.MenuItemId,
            Amount = i.Amount,
            Cost = i.Amount * i.MenuItem!.Price,
            Status = i.Status,
        }).ToList();

        return new OrderVM()
        {
            OrderId = order.Id,
            VisitId = order.VisitId,
            Cost = order.Cost,
            Status = order.Status,
            Items = orderItems ?? [],
            EmployeeId = order.EmployeeId
        };
    }

}
