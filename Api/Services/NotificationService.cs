using Reservant.ErrorCodeDocs.Attributes;
using Reservant.Api.Data;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Notification;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Microsoft.EntityFrameworkCore;
using FluentValidation.Results;
using Reservant.Api.Validators;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing notifications
/// </summary>
public class NotificationService(ApiDbContext context, FileUploadService uploadService, UserService userService)
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
<<<<<<< HEAD
    /// <summary>
    /// Notify a user that his friend request was accepted
    /// </summary>
    /// <param name="targetUserId">ID of the person to receive the notification</param>
    /// <param name="friendRequestId">ID of the friend request that was accepted</param>
    /// <returns></returns>
    public async Task NotifyFriendRequestAccepted(string targetUserId, int friendRequestId)
    {
        var request = await context.FriendRequests
            .Include(r => r.Sender)
            .Include(r => r.Receiver)
            .Where(r => r.Id == friendRequestId)
            .FirstOrDefaultAsync();

        await NotifyUser(
            targetUserId,
            new NotificationFriendRequestAccepted
            {
                FriendRequestId = request.Id,
                DateRequestSend = request.DateSent,
                SenderId = request.SenderId,
                ReceiverId = request.ReceiverId,
                SenderDetails = new Dtos.User.UserDetailsVM
                {
                    UserId = request.SenderId,
                    Login = request.Sender.UserName,
                    Email = request.Sender.Email,
                    PhoneNumber = request.Sender.PhoneNumber,
                    FirstName = request.Sender.FirstName,
                    LastName = request.Sender.LastName,
                    RegisteredAt = request.Sender.RegisteredAt,
                    Photo = uploadService.GetPathForFileName(request.Sender.PhotoFileName),
                    Roles = await userService.GetRolesAsync(request.Sender)
                },
                ReceiverDetails = new Dtos.User.UserDetailsVM
                {
                    UserId = request.ReceiverId,
                    Login = request.Receiver.UserName,
                    Email = request.Receiver.Email,
                    PhoneNumber = request.Receiver.PhoneNumber,
                    FirstName = request.Receiver.FirstName,
                    LastName = request.Receiver.LastName,
                    RegisteredAt = request.Receiver.RegisteredAt,
                    Photo = uploadService.GetPathForFileName(request.Receiver.PhotoFileName),
                    Roles = await userService.GetRolesAsync(request.Receiver)
                }
            }
            );
    }
=======

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

>>>>>>> dev
}
