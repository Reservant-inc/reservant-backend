using ErrorCodeDocs.Attributes;
using Reservant.Api.Data;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Notification;
using Reservant.Api.Validation;

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
    public async Task<Result<Pagination<NotificationVM>>> GetNotificaions(
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
                NotificationType = n.NotificationType,
                Details = n.Details,
            })
            .PaginateAsync(page, perPage, [], 100);
    }
}
