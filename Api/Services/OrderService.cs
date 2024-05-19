using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Validators;
using FluentValidation.Results;
using Reservant.Api.Validation;
using Reservant.Api.Models.Dtos.Restaurant;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Identity;

namespace Reservant.Api.Services
{
    public class OrderService(ApiDbContext context, UserManager<User> userManager)
    {
        public async Task<Result<Pagination<OrderSummaryVM>>> GetOrdersAsync(string userId, int restaurantId, bool returnFinished = false, int page = 0, int perPage = 10, OrderSorting? orderBy = null)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(userId),
                    ErrorMessage = $"User with ID {userId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            if (!await userManager.IsInRoleAsync(user, Roles.RestaurantEmployee))
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(userId),
                    ErrorMessage = $"User with ID {userId} is not a RestaurantEmployee",
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

            var isEmployeeAtRestaurant = await context.Employments.AnyAsync(e => e.EmployeeId == userId && e.RestaurantId == restaurantId && e.DateUntil == null);
            if (!isEmployeeAtRestaurant)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(userId),
                    ErrorMessage = $"User with ID {userId} is not employed at restaurant with ID {restaurantId}",
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

            var restaurant = await context.Restaurants.FindAsync(restaurantId);
            if (restaurant == null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(restaurantId),
                    ErrorMessage = $"Restaurant with ID {restaurantId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var ordersQuery = context.Orders
                .Include(order => order.Visit)
                .Include(order => order.OrderItems)
                .ThenInclude(orderItem => orderItem.MenuItem)
                .Where(order => order.Visit.TableRestaurantId == restaurantId);

            var orders = await ordersQuery.ToListAsync();

            var filteredOrders = orders.Where(order => returnFinished
                                        ? order.Status == OrderStatus.Taken || order.Status == OrderStatus.Cancelled
                                        : order.Status == OrderStatus.InProgress || order.Status == OrderStatus.Ready)
                                       .Select(order => new OrderSummaryVM
                                       {
                                           OrderId = order.Id,
                                           VisitId = order.VisitId,
                                           Note = order.Note,
                                           Cost = order.Cost,
                                           Status = order.Status
                                       });

            filteredOrders = orderBy switch
            {
                OrderSorting.DateAsc => filteredOrders.OrderBy(o => o.OrderId),
                OrderSorting.DateDesc => filteredOrders.OrderByDescending(o => o.OrderId),
                OrderSorting.CostAsc => filteredOrders.OrderBy(o => o.Cost),
                OrderSorting.CostDesc => filteredOrders.OrderByDescending(o => o.Cost),
                _ => filteredOrders
            };

            var totalRecords = filteredOrders.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / perPage);

            if (page < 0 || perPage <= 0 || page >= totalPages)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(page),
                    ErrorMessage = "Invalid page or perPage value",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var paginatedResults = filteredOrders
                .Skip(page * perPage)
                .Take(perPage)
                .ToList();

            return new Pagination<OrderSummaryVM>
            {
                Items = paginatedResults,
                TotalPages = totalPages,
                Page = page,
                PerPage = perPage
            };
        }

    }
}
