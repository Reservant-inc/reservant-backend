﻿using Reservant.ErrorCodeDocs.Attributes;
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
public class NotificationService(ApiDbContext context, FileUploadService uploadService)
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
            },
            restaurant.LogoFileName);
    }

    /// <summary>
    /// Notify a user that somebody left a review under one of their restaurants
    /// </summary>
    public async Task NotifyNewRestaurantReview(
        string targetUserId, int reviewId)
    {
        var review = await context.Reviews
            .AsNoTracking()
            .Include(r => r.Restaurant)
            .Include(r => r.Author)
            .SingleAsync(r => r.Id == reviewId);
        await NotifyUser(
            targetUserId,
            new NotificationNewRestaurantReview
            {
                RestaurantId = review.Restaurant.Id,
                RestaurantName = review.Restaurant.Name,
                ReviewId = review.Id,
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
        string targetUserId, NotificationDetails details, string? photoFileName = null)
    {
        context.Add(new Notification(DateTime.UtcNow, targetUserId, details)
        {
            PhotoFileName = photoFileName,
        });
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Notify a user that they have a new friend request
    /// </summary>
    public async Task NotifyNewFriendRequest(string senderId, string receiverId)
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
                ,sender.PhotoFileName);
        }
    }





    /// <summary>
    /// Notify a user that they have a new participation request
    /// </summary>
    public async Task NotifyNewParticipationRequest(string receiverId, string senderId, int eventId)
    {
        var sender = await context.Users
            .Where(u => u.Id == senderId)
            .Select(u => new { u.FullName, u.PhotoFileName })
            .FirstOrDefaultAsync();

        if (sender != null)
        {
            await NotifyUser(
                receiverId,
                new NotificationNewParticipationRequest
                {
                    SenderId = senderId,
                    SenderName = sender.FullName,
                    EventId = eventId,
                },
                sender.PhotoFileName); // Assuming you want to send the first photo
        }
    }

    /// <summary>
    /// Notify a user that they have a new participation request response
    /// </summary>
    public async Task NotifyNewParticipationRequestResponse(string receiverId, int eventId, bool accepted)
    {
        var eventData = await context.Events
            .Where(e => e.Id == eventId)
            .Include(e => e.Restaurant)
            .ThenInclude(r => r.Photos)
            .Select(e => new { e.Description, Photos = e.Restaurant.Photos.Select(p => p.PhotoFileName) })
            .FirstOrDefaultAsync();

        if (eventData != null)
        {
            await NotifyUser(
                receiverId,
                new NotificationNewParticipationRequestResponse
                {
                    EventId = eventId,
                    DateAccepted = accepted ? DateTime.Now : null,
                    DateDeleted = accepted ? null : DateTime.Now
                },
                eventData.Photos.FirstOrDefault());
        }
    }

}
