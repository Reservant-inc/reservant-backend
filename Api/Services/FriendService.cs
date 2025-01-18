using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using FluentValidation.Results;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.FriendRequests;
using Reservant.Api.Mapping;
using Reservant.Api.Models.Enums;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing friends and friend requests
/// </summary>
public class FriendService(ApiDbContext context, UrlService urlService, NotificationService notificationService)
{
    /// <summary>
    /// Create a friend request
    /// </summary>
    /// <param name="senderId">Sender ID</param>
    /// <param name="receiverId">Receiver ID</param>
    /// <returns></returns>
    [ErrorCode(nameof(receiverId), ErrorCodes.CannotBeCurrentUser)]
    [ErrorCode(nameof(receiverId), ErrorCodes.NotFound)]
    [ErrorCode(nameof(receiverId), ErrorCodes.Duplicate, "Friend request already exists")]
    [ErrorCode(nameof(receiverId), ErrorCodes.AlreadyFriends)]
    public async Task<Result> SendFriendRequestAsync(Guid senderId, Guid receiverId)
    {
        if (receiverId == senderId)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(receiverId),
                ErrorCode = ErrorCodes.CannotBeCurrentUser,
            };
        }

        var receiverExists = await context.Users.AnyAsync(u => u.Id == receiverId);
        if (!receiverExists)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(receiverId),
                ErrorMessage = "Receiver user does not exist",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        var userIds = new[] { senderId, receiverId };

        var existingRequest = await context.FriendRequests
            .FirstOrDefaultAsync(fr =>
                userIds.Contains(fr.SenderId) &&
                userIds.Contains(fr.ReceiverId)
            );

        if (existingRequest is null)
        {
            var request = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                DateSent = DateTime.UtcNow,
            };
            context.Add(request);

            await context.SaveChangesAsync();
            await notificationService.NotifyNewFriendRequest(senderId, request.FriendRequestId);
            return Result.Success;
        }

        if (existingRequest.DateAccepted is not null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(receiverId),
                ErrorMessage = "Users are already friends",
                ErrorCode = ErrorCodes.AlreadyFriends
            };
        }

        if (existingRequest.SenderId == receiverId)
        {
            existingRequest.DateAccepted = DateTime.UtcNow;

            await context.SaveChangesAsync();
            await notificationService.NotifyFriendRequestAccepted(existingRequest.SenderId, existingRequest.FriendRequestId);
            return Result.Success;
        }

        return new ValidationFailure
        {
            PropertyName = nameof(receiverId),
            ErrorMessage = "Friend request already exists",
            ErrorCode = ErrorCodes.Duplicate
        };
    }

    /// <summary>
    /// Mark a friend request as read
    /// </summary>
    /// <param name="receiverId">Request's receiver ID</param>
    /// <param name="senderId">Request's sender ID</param>
    [ErrorCode(nameof(receiverId), ErrorCodes.NotFound)]
    [ErrorCode(nameof(receiverId), ErrorCodes.Duplicate, "Friend request already read")]
    public async Task<Result> MarkFriendRequestAsReadAsync(Guid receiverId, Guid senderId)
    {
        var friendRequest = await context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.SenderId == senderId && fr.ReceiverId == receiverId && fr.DateDeleted == null);

        if (friendRequest == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(receiverId),
                ErrorMessage = "Friend request not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        if (friendRequest.DateRead != null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(receiverId),
                ErrorMessage = "Friend request already read",
                ErrorCode = ErrorCodes.Duplicate
            };
        }

        friendRequest.DateRead = DateTime.UtcNow;
        context.FriendRequests.Update(friendRequest);
        await context.SaveChangesAsync();
        return Result.Success;
    }

    /// <summary>
    /// Mark a friend request as accepted
    /// </summary>
    /// <param name="receiverId">Request's receiver ID</param>
    /// <param name="senderId">Request's sender ID</param>
    [ErrorCode(nameof(receiverId), ErrorCodes.CannotBeCurrentUser)]
    [ErrorCode(nameof(receiverId), ErrorCodes.NotFound)]
    [ErrorCode(nameof(receiverId), ErrorCodes.Duplicate, "Friend request already accepted")]
    public async Task<Result> AcceptFriendRequestAsync(Guid receiverId, Guid senderId)
    {
        if (receiverId == senderId)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(receiverId),
                ErrorCode = ErrorCodes.CannotBeCurrentUser,
            };
        }

        var friendRequest = await context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.SenderId == senderId && fr.ReceiverId == receiverId && fr.DateDeleted == null);

        if (friendRequest == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(receiverId),
                ErrorMessage = "Friend request not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        if (friendRequest.DateAccepted != null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(receiverId),
                ErrorMessage = "Friend request already accepted",
                ErrorCode = ErrorCodes.Duplicate
            };
        }

        friendRequest.DateAccepted = DateTime.UtcNow;
        context.FriendRequests.Update(friendRequest);
        await context.SaveChangesAsync();
        await notificationService.NotifyFriendRequestAccepted(senderId, friendRequest.FriendRequestId);
        return Result.Success;
    }

    /// <summary>
    /// Delete a friend request
    /// </summary>
    /// <param name="otherUserId">ID of the other user</param>
    /// <param name="currentUserId">ID of the current user</param>
    /// <returns></returns>
    [ErrorCode(nameof(otherUserId), ErrorCodes.CannotBeCurrentUser)]
    [ErrorCode(nameof(otherUserId), ErrorCodes.NotFound)]
    public async Task<Result> DeleteFriendAsync(Guid otherUserId, Guid currentUserId)
    {
        if (otherUserId == currentUserId)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(otherUserId),
                ErrorCode = ErrorCodes.CannotBeCurrentUser,
            };
        }

        var friendRequest = await context.FriendRequests
            .FirstOrDefaultAsync(fr =>
                fr.SenderId == currentUserId && fr.ReceiverId == otherUserId ||
                fr.ReceiverId == currentUserId && fr.SenderId == otherUserId);

        if (friendRequest == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(otherUserId),
                ErrorMessage = "Friend request not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        friendRequest.DateDeleted = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return Result.Success;
    }

    /// <summary>
    /// Get given user's friends (accepted friend requests)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="page">Page</param>
    /// <param name="perPage">Items per page</param>
    /// <returns>Paginated list of friend requests</returns>
    [MethodErrorCodes(typeof(Utils), nameof(Utils.PaginateAsync))]
    public async Task<Result<Pagination<FriendRequestWithPrivateThreadsVM>>> GetFriendsAsync(Guid userId, int page, int perPage)
    {
        var privateThreadIds = context.MessageThreads.Where(p =>
            p.Type == MessageThreadType.Private &&
            (p.CreatorId == userId) || (p.Participants.Any(p => p.Id == userId)));

        var query = context.FriendRequests
            .Where(fr => (fr.ReceiverId == userId || fr.SenderId == userId) && fr.DateAccepted != null && fr.DateDeleted == null)
            .OrderBy(fr => fr.DateAccepted)
            .Select(fr => new FriendRequestWithPrivateThreadsVM
            {
                DateSent = fr.DateSent,
                DateRead = fr.DateRead,
                DateAccepted = fr.DateAccepted,
                OtherUser = fr.ReceiverId == userId
                    ? new Dtos.Users.UserSummaryVM
                    {
                        UserId = fr.SenderId,
                        FirstName = fr.Sender.FirstName,
                        LastName = fr.Sender.LastName,
                        Photo = urlService.GetPathForFileName(fr.Sender.PhotoFileName),
                        IsArchived = fr.Sender.IsArchived
                    }
                    : new Dtos.Users.UserSummaryVM
                    {
                        UserId = fr.ReceiverId,
                        FirstName = fr.Receiver.FirstName,
                        LastName = fr.Receiver.LastName,
                        Photo = urlService.GetPathForFileName(fr.Receiver.PhotoFileName),
                        IsArchived = fr.Receiver.IsArchived
                    },
                PrivateMessageThreadId = (privateThreadIds.Where(p =>
                    ((p.CreatorId == fr.SenderId && p.Participants.Any(p => p.Id == fr.ReceiverId)) ||
                    (p.CreatorId == fr.ReceiverId && p.Participants.Any(p => p.Id == fr.SenderId))))
                    .FirstOrDefault() == null)?
                    null :
                    privateThreadIds.Where(p =>
                    ((p.CreatorId == fr.SenderId && p.Participants.Any(p => p.Id == fr.ReceiverId)) ||
                    (p.CreatorId == fr.ReceiverId && p.Participants.Any(p => p.Id == fr.SenderId))))
                    .First().MessageThreadId

            });

        return await query.PaginateAsync(page, perPage, [], 100, false);
    }

    /// <summary>
    /// Get given user's not accepted incoming friend requests
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="unreadOnly">Return only unread requests</param>
    /// <param name="page">Page</param>
    /// <param name="perPage">Items per page</param>
    /// <returns>Paginated list of friend requests</returns>
    [MethodErrorCodes(typeof(Utils), nameof(Utils.PaginateAsync))]
    public async Task<Result<Pagination<FriendRequestVM>>> GetIncomingFriendRequestsAsync(Guid userId, bool unreadOnly, int page, int perPage)
    {
        var query = context.FriendRequests
            .Where(fr => fr.ReceiverId == userId && fr.DateAccepted == null && fr.DateDeleted == null && (!unreadOnly || fr.DateRead == null))
            .OrderByDescending(fr => fr.DateSent)
            .Select(fr => new FriendRequestVM
            {
                DateSent = fr.DateSent,
                DateRead = fr.DateRead,
                DateAccepted = fr.DateAccepted,
                OtherUser = new Dtos.Users.UserSummaryVM
                {
                    UserId = fr.SenderId,
                    FirstName = fr.Sender.FirstName,
                    LastName = fr.Sender.LastName,
                    Photo = urlService.GetPathForFileName(fr.Sender.PhotoFileName),
                    IsArchived = fr.Sender.IsArchived
                },
            });

        return await query.PaginateAsync(page, perPage, [], 100, false);
    }

    /// <summary>
    /// Get given user's not accepted outgoing friend requests
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="page">Page</param>
    /// <param name="perPage">Items per page</param>
    /// <returns>Paginated list of friend requests</returns>
    [MethodErrorCodes(typeof(Utils), nameof(Utils.PaginateAsync))]
    public async Task<Result<Pagination<FriendRequestVM>>> GetOutgoingFriendRequestsAsync(Guid userId, int page, int perPage)
    {
        var query = context.FriendRequests
            .Where(fr => fr.SenderId == userId && fr.DateAccepted == null && fr.DateDeleted == null)
            .OrderByDescending(fr => fr.DateSent)
            .Select(fr => new FriendRequestVM
            {
                DateSent = fr.DateSent,
                DateRead = fr.DateRead,
                DateAccepted = fr.DateAccepted,
                OtherUser = new Dtos.Users.UserSummaryVM
                {
                    UserId = fr.ReceiverId,
                    FirstName = fr.Receiver.FirstName,
                    LastName = fr.Receiver.LastName,
                    Photo = urlService.GetPathForFileName(fr.Receiver.PhotoFileName),
                    IsArchived = fr.Receiver.IsArchived
                },
            });

        return await query.PaginateAsync(page, perPage, [], 100, false);
    }
}
