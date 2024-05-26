using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Models.Dtos.OrderItem;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using System.Security.Claims;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing orders
/// </summary>
public class OrderService(
    UserManager<User> userManager,
    ApiDbContext context,
    ValidationService validationService)
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
                .Where(e => e.EmployeeId == user!.Id)
                .ToListAsync();

            if (employment == null || !employment.Any(e => e.RestaurantId == visit!.RestaurantId))
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

    /// <summary>
    /// Returns a list of restaurants owned by the user.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Result<bool>> CancelOrderAsync(int id, User user)
    {
        var userId = user.Id;

        var result = await context
            .Orders
            .Include(o => o.Visit)
            .Include(o => o.OrderItems)
            .Where(o => o.Id == id)
            .FirstOrDefaultAsync();

        if (result == null)
              return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.NotFound
                };

        if (result.Visit != null && result.Visit.ClientId != userId)
            return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.AccessDenied
                };

        if (result.OrderItems == null || result.OrderItems.Any(oi => oi.Status == OrderStatus.Taken || oi.Status == OrderStatus.Cancelled))
            return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.SomeOfItemsAreTaken
                };

        foreach (var orderItem in result.OrderItems)
        {
            orderItem.Status = OrderStatus.Cancelled;
        }

        await context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Creating new order
    /// </summary>
    public async Task<Result<OrderSummaryVM>> CreateOrderAsync(CreateOrderRequest request, User user)
    {
        var menuItemIds = request.Items.Select(i => i.MenuItemId).ToList();
        var menuItems = await context.MenuItems
            .Where(mi => menuItemIds.Contains(mi.Id))
            .ToListAsync();

        var result = await validationService.ValidateAsync(request, user.Id);

        if (!result.IsValid)
        {
            return result;
        }

        var visit = await context.Visits
            .Where(v => v.Id == request.VisitId)
            .Include(visit => visit.Participants)
            .FirstOrDefaultAsync();

        if (visit.ClientId != user.Id && !visit.Participants.Contains(user))
        {
            return new ValidationFailure
            {
                PropertyName = nameof(user.Id),
                AttemptedValue = user.Id,
                ErrorCode = ErrorCodes.UserDoesNotParticipateInVisit
            };
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

        result = await validationService.ValidateAsync(order, user.Id);
        if (!result.IsValid)
        {
            return result;
        }

        context.Add(order);
        await context.SaveChangesAsync();


        return new OrderSummaryVM
        {
            OrderId = order.Id,
            Cost = order.Cost,
            Note = order.Note,
            Status = order.Status,
            VisitId = order.VisitId,
            Date = visit.Date
        };
    }
}
