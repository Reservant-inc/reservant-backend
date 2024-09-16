using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using FluentValidation.Results;
using Reservant.Api.Validators;
using ErrorCodeDocs.Attributes;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.FriendRequest;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing friends and friend requests
/// </summary>
public class FriendService(ApiDbContext context, FileUploadService uploadService)
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
    public async Task<Result> SendFriendRequestAsync(string senderId, string receiverId)
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

        var userIds = new List<string>() { senderId, receiverId };

        var areFriends = await context.FriendRequests
            .AnyAsync(fr => 
                fr.DateDeleted == null &&
                fr.DateAccepted != null &&
                userIds.Contains(fr.SenderId) &&
                userIds.Contains(fr.ReceiverId)
            );

        if (areFriends)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(receiverId),
                ErrorMessage = "Users are already friends",
                ErrorCode = ErrorCodes.AlreadyFriends
            };
        }

        var existingRequest = await context.FriendRequests
            .AnyAsync(fr => fr.SenderId == senderId && fr.ReceiverId == receiverId && fr.DateDeleted == null);

        if (existingRequest)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(receiverId),
                ErrorMessage = "Friend request already exists",
                ErrorCode = ErrorCodes.Duplicate
            };
        }

        var requestFromReceiver = await context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.SenderId == receiverId && fr.ReceiverId == senderId && fr.DateDeleted == null);

        var friendRequest = new FriendRequest
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            DateSent = DateTime.UtcNow
        };

        if (requestFromReceiver != null)
        {
            requestFromReceiver.DateAccepted = DateTime.UtcNow;
            friendRequest.DateAccepted = DateTime.UtcNow;
        }

        context.FriendRequests.Add(friendRequest);
        await context.SaveChangesAsync();
        return Result.Success;
    }

    /// <summary>
    /// Mark a friend request as read
    /// </summary>
    /// <param name="receiverId">Request's receiver ID</param>
    /// <param name="senderId">Request's sender ID</param>
    [ErrorCode(nameof(receiverId), ErrorCodes.NotFound)]
    [ErrorCode(nameof(receiverId), ErrorCodes.Duplicate, "Friend request already read")]
    public async Task<Result> MarkFriendRequestAsReadAsync(string receiverId, string senderId)
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
    public async Task<Result> AcceptFriendRequestAsync(string receiverId, string senderId)
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
    public async Task<Result> DeleteFriendAsync(string otherUserId, string currentUserId)
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
    public async Task<Result<Pagination<FriendRequestVM>>> GetFriendsAsync(string userId, int page, int perPage)
    {
        var query = context.FriendRequests
            .Where(fr => (fr.ReceiverId == userId || fr.SenderId == userId) && fr.DateAccepted != null && fr.DateDeleted == null)
            .OrderBy(fr => fr.DateAccepted)
            .Select(fr => new FriendRequestVM
            {
                DateSent = fr.DateSent,
                DateRead = fr.DateRead,
                DateAccepted = fr.DateAccepted,
                OtherUser = fr.ReceiverId == userId
                    ? new Dtos.User.UserSummaryVM
                    {
                        UserId = fr.SenderId,
                        FirstName = fr.Sender.FirstName,
                        LastName = fr.Sender.LastName,
                        Photo = uploadService.GetPathForFileName(fr.Sender.PhotoFileName),
                    }
                    : new Dtos.User.UserSummaryVM
                    {
                        UserId = fr.ReceiverId,
                        FirstName = fr.Receiver.FirstName,
                        LastName = fr.Receiver.LastName,
                        Photo = uploadService.GetPathForFileName(fr.Receiver.PhotoFileName),
                    },
            });

        return await query.PaginateAsync(page, perPage, []);
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
    public async Task<Result<Pagination<FriendRequestVM>>> GetIncomingFriendRequestsAsync(string userId, bool unreadOnly, int page, int perPage)
    {
        var query = context.FriendRequests
            .Where(fr => fr.ReceiverId == userId && fr.DateAccepted == null && fr.DateDeleted == null && (!unreadOnly || fr.DateRead==null))
            .OrderByDescending(fr => fr.DateSent)
            .Select(fr => new FriendRequestVM
            {
                DateSent = fr.DateSent,
                DateRead = fr.DateRead,
                DateAccepted = fr.DateAccepted,
                OtherUser = new Dtos.User.UserSummaryVM
                {
                    UserId = fr.SenderId,
                    FirstName = fr.Sender.FirstName,
                    LastName = fr.Sender.LastName,
                    Photo = uploadService.GetPathForFileName(fr.Sender.PhotoFileName),
                },
            });

        return await query.PaginateAsync(page, perPage, []);
    }

    /// <summary>
    /// Get given user's not accepted outgoing friend requests
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="page">Page</param>
    /// <param name="perPage">Items per page</param>
    /// <returns>Paginated list of friend requests</returns>
    [MethodErrorCodes(typeof(Utils), nameof(Utils.PaginateAsync))]
    public async Task<Result<Pagination<FriendRequestVM>>> GetOutgoingFriendRequestsAsync(string userId, int page, int perPage)
    {
        var query = context.FriendRequests
            .Where(fr => fr.SenderId == userId && fr.DateAccepted == null && fr.DateDeleted == null)
            .OrderByDescending(fr => fr.DateSent)
            .Select(fr => new FriendRequestVM
            {
                DateSent = fr.DateSent,
                DateRead = fr.DateRead,
                DateAccepted = fr.DateAccepted,
                OtherUser  = new Dtos.User.UserSummaryVM
                {
                    UserId = fr.ReceiverId,
                    FirstName = fr.Receiver.FirstName,
                    LastName = fr.Receiver.LastName,
                    Photo = uploadService.GetPathForFileName(fr.Receiver.PhotoFileName),
                },
            });

        return await query.PaginateAsync(page, perPage, []);
    }
}
