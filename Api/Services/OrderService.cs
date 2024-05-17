using Microsoft.EntityFrameworkCore;
namespace Reservant.Api.Services;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Validation;


/// <summary>
/// Service for managing orders
/// </summary>
public class OrderService(ApiDbContext context)
{
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
            .Where(o => o.Id == id && o.Visit!=null?o.Visit.ClientId == userId:false)
            .Where(o => o.OrderItems!=null?(!o.OrderItems.Any(oi => oi.Status == OrderStatus.Taken) && !o.OrderItems.Any(oi => oi.Status == OrderStatus.Cancelled)):false)
            .FirstOrDefaultAsync();

        if (result == null)
            return false;

        foreach (var orderItem in result.OrderItems)
        {
            orderItem.Status = OrderStatus.Cancelled;
        }

        await context.SaveChangesAsync();

        return true;
    }

}
