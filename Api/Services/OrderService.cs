using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Models.Dtos.OrderItem;
using Reservant.Api.Validation;
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing orders
/// </summary>
public class OrderService(ApiDbContext context)
{
    /// <summary>
    /// Gets the order with the given id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Result<OrderVM>> GetOrderById(int id)
    {

        var errors = new List<ValidationResult>();

        var order = await context.Orders
            .Include(o => o.OrderItems!)
            .ThenInclude(oi => oi.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            errors.Add(new ValidationResult($"Order: {id} not found"));
            return errors;
        }

        var orderItems = order.OrderItems?.Select(i => new OrderItemVM()
        {
            MenuItemId = i.MenuItemId,
            Amount = i.Amount,
            Cost = i.Amount * i.MenuItem?.Price ?? 0,
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
