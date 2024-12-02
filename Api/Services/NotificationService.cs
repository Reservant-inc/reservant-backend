using System.Text.Json;
using Reservant.ErrorCodeDocs.Attributes;
using Reservant.Api.Data;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Notifications;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Mapping;
using Reservant.Api.Push;
using Reservant.Api.Identity;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing notifications
/// </summary>
public class NotificationService(
    ApiDbContext context,
    UrlService urlService,
    PushService pushService,
    FirebaseBackgroundService firebaseService)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

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
            .OrderByDescending(n => n.NotificationId)
            .Select(n => new NotificationVM
            {
                NotificationId = n.NotificationId,
                DateCreated = n.DateCreated,
                DateRead = n.DateRead,
                Photo = urlService.GetPathForFileName(n.PhotoFileName),
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
            .Where(n => dto.NotificationIds.Contains(n.NotificationId) && n.DateRead == null)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(n => n.DateRead, _ => DateTime.UtcNow));
    }

    /// <summary>
    /// Notify a user that a restaurant has been verified
    /// </summary>
    public async Task NotifyRestaurantVerified(
        Guid targetUserId, int restaurantId)
    {
        var restaurant = await context.Restaurants
            .AsNoTracking()
            .SingleAsync(r => r.RestaurantId == restaurantId);
        await NotifyUser(
            targetUserId,
            new NotificationRestaurantVerified
            {
                RestaurantId = restaurant.RestaurantId,
                RestaurantName = restaurant.Name,
            },
            restaurant.LogoFileName);
    }

    /// <summary>
    /// Notify a user that somebody left a review under one of their restaurants
    /// </summary>
    public async Task NotifyNewRestaurantReview(
        Guid targetUserId, int reviewId)
    {
        var review = await context.Reviews
            .AsNoTracking()
            .Include(r => r.Restaurant)
            .Include(r => r.Author)
            .SingleAsync(r => r.ReviewId == reviewId);
        await NotifyUser(
            targetUserId,
            new NotificationNewRestaurantReview
            {
                RestaurantId = review.Restaurant.RestaurantId,
                RestaurantName = review.Restaurant.Name,
                ReviewId = review.ReviewId,
                Stars = review.Stars,
                Contents = review.Contents,
                AuthorId = review.AuthorId,
                AuthorName = review.Author.FullName,
            },
            review.Restaurant.LogoFileName);
    }

    /// <summary>
    /// Notify a user that something has happened
    /// </summary>
    /// <param name="targetUserId">ID of the user that will receive the notification</param>
    /// <param name="details">Kind-specific information. Stored in JSON in the database</param>
    /// <param name="photoFileName">
    /// File name of a picture related to the notification.
    /// For example a user's profile picture, or a restaurant's logo
    /// </param>
    private async Task NotifyUser(
        Guid targetUserId, NotificationDetails details, string? photoFileName = null)
    {
        await NotifyUsers(Enumerable.Repeat(targetUserId, 1), details, photoFileName);
    }

    /// <summary>
    /// Notify multiple users that something has happened
    /// </summary>
    /// <param name="targetUserIds">IDs of the users that will receive the notification</param>
    /// <param name="details">Kind-specific information. Stored in JSON in the database</param>
    /// <param name="photoFileName">
    /// File name of a picture related to the notification.
    /// For example a user's profile picture, or a restaurant's logo
    /// </param>
    /// <param name="storeNotification">
    /// Whether to persist the notification in the database.
    /// Not required for example for New Message notifications, since those just
    /// repeat message objects.
    /// </param>
    private async Task NotifyUsers(
        IEnumerable<Guid> targetUserIds,
        NotificationDetails details,
        string? photoFileName = null,
        bool storeNotification = true)
    {
        byte[]? pushMessage = null;
        foreach (var targetUserId in targetUserIds)
        {
            var notification = new Notification(DateTime.UtcNow, targetUserId, details)
            {
                PhotoFileName = photoFileName,
            };

            if (storeNotification)
            {
                context.Add(notification);
            }

            pushMessage ??= JsonSerializer.SerializeToUtf8Bytes(new NotificationVM
            {
                NotificationId = notification.NotificationId,
                DateCreated = notification.DateCreated,
                DateRead = notification.DateRead,
                Photo = urlService.GetPathForFileName(notification.PhotoFileName),
                NotificationType = notification.Details.GetType().Name,
                Details = notification.Details,
            }, JsonOptions);

            pushService.SendToUser(targetUserId, pushMessage);
            firebaseService.EnqueueNotification(notification);
        }

        if (storeNotification)
        {
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Notify multiple users that they have received a new message
    /// </summary>
    /// <param name="targetUserIds">IDs of the users</param>
    /// <param name="message">The new message</param>
    public async Task NotifyNewMessage(IEnumerable<Guid> targetUserIds, Message message)
    {
        await NotifyUsers(
            targetUserIds,
            new NotificationNewMessage
            {
                MessageId = message.MessageId,
                ThreadId = message.MessageThreadId,
                AuthorId = message.AuthorId,
                AuthorName = message.Author.FullName,
                ThreadTitle = message.MessageThread.Title,
                Contents = message.Contents,
            },
            message.Author.PhotoFileName,
            storeNotification: false);
    }

    /// <summary>
    /// Notify employees that there is a new reservation in a restaurant
    /// </summary>
    /// <param name="targetUserIds">IDs of the employees</param>
    /// <param name="reservation">R</param>
    public async Task NotifyNewReservation(IEnumerable<Guid> targetUserIds, Visit reservation)
    {
        await NotifyUsers(
            targetUserIds,
            new NotificationNewReservation
            {
                RestaurantId = reservation.RestaurantId,
                RestaurantName = reservation.Restaurant.Name,
                Takeaway = reservation.Takeaway,
                Date = reservation.Reservation!.StartTime,
                EndTime = reservation.Reservation!.EndTime,
                NumberOfPeople = reservation.NumberOfGuests + reservation.Participants.Count + 1,
            });
    }

    /// <summary>
    /// Notify a user that his friend request was accepted
    /// </summary>
    /// <param name="targetUserId">ID of the person to receive the notification</param>
    /// <param name="friendRequestId">ID of the friend request that was accepted</param>
    /// <returns></returns>
    public async Task NotifyFriendRequestAccepted(Guid targetUserId, int friendRequestId)
    {
        var request = await context.FriendRequests
            .Include(r => r.Receiver)
            .Where(r => r.FriendRequestId == friendRequestId)
            .SingleAsync();

        await NotifyUser(
            targetUserId,
            new NotificationFriendRequestAccepted
            {
                FriendRequestId = request.FriendRequestId,
                AcceptingUserId = request.SenderId,
                AcceptingUserFullName = request.Receiver.FullName
            },
            request.Receiver.PhotoFileName);
    }

    /// <summary>
    /// Notify a user that they have a new friend request
    /// </summary>
    public async Task NotifyNewFriendRequest(Guid targetUserId, int friendRequestId)
    {
        var request = await context.FriendRequests
            .Include(request => request.Sender)
            .SingleAsync(request => request.FriendRequestId == friendRequestId);

        await NotifyUser(
            targetUserId,
            new NotificationNewFriendRequest
            {
                FriendRequestId = friendRequestId,
                SenderId = request.SenderId,
                SenderName = request.Sender.FullName,
            },
            request.Sender.PhotoFileName);
    }

    /// <summary>
    /// Notify a user that somebody wants to participate in their event
    /// </summary>
    public async Task NotifyNewParticipationRequest(Guid receiverId, Guid senderId, int eventId)
    {
        var eventName = await context.Events
            .Where(e => e.EventId == eventId)
            .Select(e => e.Name)
            .SingleAsync();

        var sender = await context.Users
            .Where(u => u.Id == senderId)
            .Select(u => new { u.FullName, u.PhotoFileName })
            .SingleAsync();

        await NotifyUser(
            receiverId,
            new NotificationNewParticipationRequest
            {
                SenderId = senderId,
                SenderName = sender.FullName,
                EventId = eventId,
                EventName = eventName,
            },
            sender.PhotoFileName);
    }

    /// <summary>
    /// Notify a user that their participation request was accepted/rejected
    /// </summary>
    public async Task NotifyParticipationRequestResponse(Guid receiverId, int eventId, bool isAccepted)
    {
        var eventData = await context.Events
            .Where(e => e.EventId == eventId)
            .Select(e => new
            {
                e.Name,
                e.Creator.PhotoFileName,
                e.CreatorId,
                CreatorName = e.Creator.FirstName + " " + e.Creator.LastName,
            })
            .FirstOrDefaultAsync();

        if (eventData != null)
        {
            await NotifyUser(
                receiverId,
                new NotificationParticipationRequestResponse
                {
                    EventId = eventId,
                    Name = eventData.Name,
                    IsAccepted = isAccepted,
                    CreatorId = eventData.CreatorId,
                    CreatorName = eventData.CreatorName,
                },
                eventData.PhotoFileName);
        }
    }

    /// <summary>
    /// Notify a user that their visit request was accepted/rejected
    /// </summary>
    public async Task NotifyVisitApprovedDeclined(Guid receiver, int visitId)
    {
        var visitData = await context.Visits
            .Where(v => v.VisitId == visitId)
            .Select(v => new
            {
                photoFileName =  v.Client.PhotoFileName,
                IsAccepted = v.Reservation!.Decision!.IsAccepted,
                RestaurantName = v.Restaurant.Name,
                Date = v.Reservation!.StartTime,
            })
            .FirstOrDefaultAsync();

        if (visitData != null)
        {
            await NotifyUser(
                receiver,
                new NotificationVisitApprovedDeclined
                {
                    VisitId = visitId,
                    IsAccepted = visitData.IsAccepted,
                    RestaurantName = visitData.RestaurantName,
                    Date = visitData.Date
                },
                visitData.photoFileName);
        }
    }

    /// <summary>
    /// Notifies all customer support managers about a new escalated report
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task NotifyNewEscalatedReport(int reportId) {
        var managers =
            from user in context.Users
            join userRole in context.UserRoles on user.Id equals userRole.UserId
            join role in context.Roles on userRole.RoleId equals role.Id
            where role.Name == Roles.CustomerSupportManager
            select user;
        foreach (var manager in managers)
        {
            await NotifyUser(
                manager.Id,
                new NotificationNewEscalatedReportDetails { 
                    reportId = reportId,
                    EscalationTime = DateTime.UtcNow
                }
                );
        }
    }
}
