using Reservant.ErrorCodeDocs.Attributes;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Orders;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using System.Security.Claims;
using AutoMapper;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing orders
/// </summary>
public class OrderService(
    UserManager<User> userManager,
    ApiDbContext context,
    ValidationService validationService,
    AuthorizationService authorizationService,
    IMapper mapper)
{
    /// <summary>
    /// Gets the order with the given id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="claim">User claims</param>
    /// <returns></returns>
    public async Task<Result<OrderVM>> GetOrderById(int id, ClaimsPrincipal claim)
    {

        var order = await context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.AssignedEmployee)
            .FirstOrDefaultAsync(o => o.OrderId == id);

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
            .Include(v => v.Restaurant)
            .ThenInclude(v => v.Group)
            .FirstOrDefaultAsync(v => v.Orders.Contains(order));

        var user = await userManager.GetUserAsync(claim);
        var roles = await userManager.GetRolesAsync(user!);

        if (roles.Contains(Roles.RestaurantOwner))
        {
            if (visit!.Restaurant.Group.OwnerId != user!.Id)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }
        }
        else if (roles.Contains(Roles.Customer)
            && visit!.ClientId != user!.Id
            && !visit.Participants.Contains(user))
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.AccessDenied
            };
        }
        else if (roles.Contains(Roles.RestaurantEmployee))
        {
            var employment = await context.Employments
                .Where(e => e.EmployeeId == user!.Id)
                .ToListAsync();

            if (!employment.Any(e => e.RestaurantId == visit!.RestaurantId))
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }
        }

        return mapper.Map<OrderVM>(order);
    }

    /// <summary>
    /// Returns a list of restaurants owned by the user.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Result> CancelOrderAsync(int id, User user)
    {
        var userId = user.Id;

        var result = await context
            .Orders
            .Include(o => o.Visit)
            .Include(o => o.OrderItems)
            .Where(o => o.OrderId == id)
            .FirstOrDefaultAsync();

        if (result == null)
              return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.NotFound
                };

        if (result.Visit.ClientId != userId)
            return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.AccessDenied
                };

        if (result.OrderItems.Any(oi => oi.Status == OrderStatus.Taken || oi.Status == OrderStatus.Cancelled))
            return new ValidationFailure
                {
                    ErrorCode = ErrorCodes.SomeOfItemsAreTaken
                };

        foreach (var orderItem in result.OrderItems)
        {
            orderItem.Status = OrderStatus.Cancelled;
        }

        await context.SaveChangesAsync();

        return Result.Success;
    }

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
            Price = menuItems[c.MenuItemId].Price,
            Status = OrderStatus.InProgress,
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

        return mapper.Map<OrderSummaryVM>(order);
    }
    /// <summary>
    /// Update status of the order by updating the list of states of included menu items
    /// </summary>
    /// <param name="id">order id</param>
    /// <param name="request">request containing list of updated menu items and employee that work on them</param>
    /// <param name="user"></param>
    /// <returns></returns>
    [ErrorCode(null, ErrorCodes.NotFound, "Order not found")]
    [ErrorCode(null, ErrorCodes.OrderIsFinished, "All items are either taken or cancelled")]
    [ErrorCode(nameof(request.Items), ErrorCodes.NotFound, "Menu item not found in the order")]
    [ErrorCode(nameof(request.EmployeeId), ErrorCodes.NotFound, "Employee not found")]
    [MethodErrorCodes(typeof(AuthorizationService), nameof(authorizationService.VerifyRestaurantHallAccess))]
    public async Task<Result<OrderVM>> UpdateOrderStatusAsync(int id, UpdateOrderStatusRequest request, User user)
    {
        var order = await context.Orders
            .Where(o => o.OrderId == id)
            .Include(o => o.AssignedEmployee)
            .Include(o => o.OrderItems)
                .ThenInclude(o => o.MenuItem)
            .Include(o => o.Visit)
                .ThenInclude(v => v.Restaurant)
                .ThenInclude(r => r.Group)
            .AsSplitQuery()
            .FirstOrDefaultAsync();

        // Check if the order exists
        if (order is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = "Order not found"
            };
        }

        if (order.OrderItems.All(item => item.Status == OrderStatus.Taken || item.Status == OrderStatus.Cancelled))
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.OrderIsFinished,
                ErrorMessage = "All items are either taken or cancelled"
            };
        }

        var authResult1 = await authorizationService.VerifyRestaurantHallAccess(order.Visit.Restaurant.RestaurantId, user.Id);
        if (authResult1.IsError)
        {
            return authResult1.Errors;
        }

        // Update the status of the order items
        foreach (var item in request.Items)
        {
            var menuItem = order.OrderItems.FirstOrDefault(x => x.MenuItemId == item.MenuItemId);
            if (menuItem is null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(request.Items),
                    ErrorCode = ErrorCodes.NotFound,
                    ErrorMessage = "Menu item not found in the order"
                };
            }

            menuItem.Status = item.Status;
        }

        // Assign employee to the order if they exist and belong to the correct restaurant group
        var employee = await context.Users
            .Include(x => x.Employments)
            .FirstOrDefaultAsync(x => x.Id == request.EmployeeId);

        if (employee is null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.EmployeeId),
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = "Employee not found"
            };
        }

        var authResult2 = await authorizationService.VerifyRestaurantHallAccess(order.Visit.Restaurant.RestaurantId, request.EmployeeId);
        if (authResult2.IsError)
        {
            return authResult2.Errors;
        }

        // Assign employee if not already assigned
        if (order.AssignedEmployee != employee)
        {
            order.AssignedEmployee = employee;
        }

        await context.SaveChangesAsync();

        return mapper.Map<OrderVM>(order);
    }
}
