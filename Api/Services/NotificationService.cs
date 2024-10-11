using System.Text.Json;
using Reservant.ErrorCodeDocs.Attributes;
using Reservant.Api.Data;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Notifications;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Push;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing notifications
/// </summary>
public class NotificationService(
    ApiDbContext context,
    FileUploadService uploadService,
    PushService pushService,
    FirebaseService firebaseService)
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
                Photo = uploadService.GetPathForFileName(n.PhotoFileName),
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
        var notification = new Notification(DateTime.UtcNow, targetUserId, details)
        {
            PhotoFileName = photoFileName,
        };
        context.Add(notification);
        await context.SaveChangesAsync();

        pushService.SendToUser(targetUserId, JsonSerializer.SerializeToUtf8Bytes(new NotificationVM
        {
            NotificationId = notification.NotificationId,
            DateCreated = notification.DateCreated,
            DateRead = notification.DateRead,
            Photo = uploadService.GetPathForFileName(notification.PhotoFileName),
            NotificationType = notification.Details.GetType().Name,
            Details = notification.Details,
        }, JsonOptions));

        await firebaseService.SendNotification(notification);
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
    public async Task NotifyNewFriendRequest(Guid senderId, Guid receiverId)
    {
        var sender = await context.Users
            .Where(u => u.Id == senderId)
            .Select(u => new { u.FullName, u.PhotoFileName })
            .FirstOrDefaultAsync();

        if (sender != null)
        {
            await NotifyUser(
                receiverId,
                new NotificationNewFriendRequest
                {
                    SenderId = senderId,
                    SenderName = sender.FullName,
                }
                , sender.PhotoFileName);
        }
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
    public async Task NotifyVisitConsiderationRequestResponse(Guid receiver, int visitId)
    {
        var visitData = await context.Visits
            .Where(v => v.VisitId == visitId)
            .Select(v => new
            {
                photoFileName =  v.Restaurant.LogoFileName,
                AnsweredById = v.AnsweredById,
                AnsweredByName = v.AnsweredBy.FirstName + " " + v.AnsweredBy.LastName,
                IsAccepted = v.IsAccepted
            })
            .FirstOrDefaultAsync();

        if (visitData != null)
        {
            await NotifyUser(
                receiver,
                new NotificationVisitConsiderationRequestResponse
                {
                    VisitId = visitId,
                    IsAccepted = visitData.IsAccepted ?? false,
                    AnsweredById = visitData.AnsweredById ?? Guid.Empty,
                    AnsweredByName = visitData.AnsweredByName,
                },
                visitData.photoFileName);
        }
    }
}
