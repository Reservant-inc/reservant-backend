using ErrorCodeDocs.Attributes;
using Reservant.Api.Data;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Notification;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Microsoft.EntityFrameworkCore;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing notifications
/// </summary>
public class NotificationService(ApiDbContext context)
{
    /// <summary>
    /// Get all notifications
    /// </summary>
    /// <param name="unreadOnly">Return only unread notifications</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page (max 100)</param>
    [MethodErrorCodes(typeof(Utils), nameof(Utils.PaginateAsync))]
    public async Task<Result<Pagination<NotificationVM>>> GetNotifications(
        bool unreadOnly, int page, int perPage)
    {
        var query = context.Notifications.AsQueryable();

        if (unreadOnly)
        {
            query = query.Where(n => n.DateRead == null);
        }

        return await query
            .OrderByDescending(n => n.Id)
            .Select(n => new NotificationVM
            {
                NotificationId = n.Id,
                DateCreated = n.DateCreated,
                DateRead = n.DateRead,
                NotificationType = n.Details.GetType().Name,
                Details = n.Details,
            })
            .PaginateAsync(page, perPage, [], 100);
    }

    /// <summary>
    /// Get data to show as notification bubbles
    /// </summary>
    public async Task<NotificationBubblesVM> GetBubbles()
    {
        return new NotificationBubblesVM
        {
            UnreadNotificationCount = await context.Notifications
                .Where(n => n.DateRead == null)
                .CountAsync(),
        };
    }

    /// <summary>
    /// Mark notifications as read.
    /// </summary>
    /// <remarks>
    /// Does not check the IDs, does not update already read notifications.
    /// </remarks>
    public async Task MarkRead(MarkNotificationsReadDto dto)
    {
        await context.Notifications
            .Where(n => dto.NotificationIds.Contains(n.Id) && n.DateRead == null)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(n => n.DateRead, _ => DateTime.UtcNow));
    }

    /// <summary>
    /// Notify a user that a restaurant has been verified
    /// </summary>
    public async Task NotifyRestaurantVerified(
        string targetUserId, int restaurantId)
    {
        var restaurant = await context.Restaurants
            .AsNoTracking()
            .SingleAsync(r => r.Id == restaurantId);
        await NotifyUser(
            targetUserId,
            new NotificationRestaurantVerified
            {
                RestaurantId = restaurant.Id,
                RestaurantName = restaurant.Name,
                RestaurantLogo = uploadService.GetPathForFileName(restaurant.LogoFileName),
            });
    }

    /// <summary>
    /// Notify a user that something has happened
    /// </summary>
    private async Task NotifyUser(
        string targetUserId, NotificationDetails details)
    {
        context.Add(new Notification(DateTime.UtcNow, targetUserId, details));
        await context.SaveChangesAsync();
    }
}
