using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.FriendRequest;
using Reservant.Api.Validation;
using FluentValidation.Results;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Validators;
using ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing friends and friend requests
/// </summary>
public class FriendService(ApiDbContext context)
{
    /// <summary>
    /// Create a friend request
    /// </summary>
    /// <param name="senderId">Sender ID</param>
    /// <param name="receiverId">Receiver ID</param>
    /// <returns></returns>
    [ErrorCode("<receiverId>", ErrorCodes.NotFound)]
    [ErrorCode("<receiverId>", ErrorCodes.Duplicate, "Friend request already exists")]
    public async Task<Result> SendFriendRequestAsync(string senderId, string receiverId)
    {
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

        var existingRequest = await context.FriendRequests
            .FirstOrDefaultAsync(fr =>
                (fr.SenderId == senderId && fr.ReceiverId == receiverId && fr.DateDeleted == null) ||
                (fr.SenderId == receiverId && fr.ReceiverId == senderId && fr.DateDeleted == null)
            );

        if (existingRequest != null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(receiverId),
                ErrorMessage = "Friend request already exists",
                ErrorCode = ErrorCodes.Duplicate
            };
        }

        var friendRequest = new FriendRequest
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            DateSent = DateTime.UtcNow
        };

        context.FriendRequests.Add(friendRequest);
        await context.SaveChangesAsync();
        return Result.Success;
    }

    /// <summary>
    /// Mark a friend request as read
    /// </summary>
    /// <param name="receiverId">Request's receiver ID</param>
    /// <param name="senderId">Request's sender ID</param>
    [ErrorCode("<receiverId>", ErrorCodes.NotFound)]
    [ErrorCode("<receiverId>", ErrorCodes.Duplicate, "Friend request already read")]
    public async Task<Result> MarkFriendRequestAsReadAsync(string receiverId, string senderId)
    {
        var friendRequest = await context.FriendRequests
            .FirstOrDefaultAsync(fr =>
                fr.SenderId == senderId && fr.ReceiverId == receiverId && fr.DateDeleted == null);

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
    [ErrorCode("<receiverId>", ErrorCodes.NotFound)]
    [ErrorCode("<receiverId>", ErrorCodes.Duplicate, "Friend request already answered")]
    public async Task<Result> AcceptFriendRequestAsync(string receiverId, string senderId)
    {
        var friendRequest = await context.FriendRequests
            .FirstOrDefaultAsync(fr =>
                fr.SenderId == senderId && fr.ReceiverId == receiverId && fr.DateDeleted == null);

        if (friendRequest == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(receiverId),
                ErrorMessage = "Friend request not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        if (friendRequest.DateAnswered != null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(receiverId),
                ErrorMessage = "Friend request already answered",
                ErrorCode = ErrorCodes.Duplicate
            };
        }

        friendRequest.IsAccepted = true;
        friendRequest.DateAnswered = DateTime.UtcNow;
        context.FriendRequests.Update(friendRequest);
        await context.SaveChangesAsync();
        return Result.Success;
    }


    /// <summary>
    /// Delete a friend request
    /// </summary>
    /// <param name="receiverId">Request's receiver ID</param>
    /// <param name="senderId">Request's sender ID</param>
    /// <returns></returns>
    [ErrorCode("<reveiverId>", ErrorCodes.NotFound)]
    public async Task<Result> DeleteFriendAsync(string receiverId, string senderId)
    {
        var friendRequest = await context.FriendRequests
            .FirstOrDefaultAsync(fr =>
                fr.SenderId == senderId && fr.ReceiverId == receiverId && fr.DateDeleted == null);

        if (friendRequest == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(receiverId),
                ErrorMessage = "Friend request not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        friendRequest.DateDeleted = DateTime.UtcNow;
        context.FriendRequests.Update(friendRequest);
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
    public async Task<Result<Pagination<FriendRequestVM>>> GetFriendsAsync(string userId, int page, int perPage)
    {
        var query = context.FriendRequests
            .Where(fr => (fr.ReceiverId == userId || fr.SenderId == userId) && fr.IsAccepted == true &&
                         fr.DateDeleted == null)
            .OrderBy(fr => fr.DateAnswered)
            .Select(fr => new FriendRequestVM
            {
                DateSent = fr.DateSent,
                DateRead = fr.DateRead,
                DateAnswered = fr.DateAnswered,
                IsAccepted = fr.IsAccepted,
                SenderId = fr.SenderId,
                ReceiverId = fr.ReceiverId,
                SenderName = fr.Sender.FullName,
                ReceiverName = fr.Receiver.FullName
            });

        return await query.PaginateAsync(page, perPage, []);
    }


    /// <summary>
    /// Get given user's not accepted incoming friend requests
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="page">Page</param>
    /// <param name="perPage">Items per page</param>
    /// <returns>Paginated list of friend requests</returns>
    public async Task<Result<Pagination<FriendRequestVM>>> GetIncomingFriendRequestsAsync(string userId, int page,
        int perPage)
    {
        var query = context.FriendRequests
            .Where(fr => fr.ReceiverId == userId && fr.DateAnswered == null && fr.DateDeleted == null)
            .OrderByDescending(fr => fr.DateSent)
            .Select(fr => new FriendRequestVM
            {
                DateSent = fr.DateSent,
                DateRead = fr.DateRead,
                DateAnswered = fr.DateAnswered,
                IsAccepted = fr.IsAccepted,
                SenderId = fr.SenderId,
                ReceiverId = fr.ReceiverId,
                SenderName = fr.Sender.FullName,
                ReceiverName = fr.Receiver.FullName
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
    public async Task<Result<Pagination<FriendRequestVM>>> GetOutgoingFriendRequestsAsync(string userId, int page,
        int perPage)
    {
        var query = context.FriendRequests
            .Where(fr => fr.SenderId == userId && fr.DateAnswered == null && fr.DateDeleted == null)
            .OrderByDescending(fr => fr.DateSent)
            .Select(fr => new FriendRequestVM
            {
                DateSent = fr.DateSent,
                DateRead = fr.DateRead,
                DateAnswered = fr.DateAnswered,
                IsAccepted = fr.IsAccepted,
                SenderId = fr.SenderId,
                ReceiverId = fr.ReceiverId,
                SenderName = fr.Sender.FullName,
                ReceiverName = fr.Receiver.FullName
            });

        return await query.PaginateAsync(page, perPage, []);
    }


    /// <summary>
    /// Mark a friend request as rejected
    /// </summary>
    /// <param name="receiverId">Request's receiver ID</param>
    /// <param name="senderId">Request's sender ID</param>
    [ErrorCode("<receiverId>", ErrorCodes.NotFound)]
    [ErrorCode("<receiverId>", ErrorCodes.Duplicate, "Friend request already answered")]
    public async Task<Result> RejectFriendRequestAsync(string receiverId, string senderId)
    {
        var friendRequest = await context.FriendRequests
            .FirstOrDefaultAsync(fr =>
                fr.SenderId == senderId && fr.ReceiverId == receiverId && fr.DateDeleted == null);

        if (friendRequest == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(receiverId),
                ErrorMessage = "Friend request not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        if (friendRequest.DateAnswered != null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(receiverId),
                ErrorMessage = "Friend request already answered",
                ErrorCode = ErrorCodes.Duplicate
            };
        }

        friendRequest.IsAccepted = false;
        friendRequest.DateAnswered = DateTime.UtcNow;
        context.FriendRequests.Update(friendRequest);
        await context.SaveChangesAsync();
        return Result.Success;
    }
}