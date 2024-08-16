﻿using Microsoft.EntityFrameworkCore;
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
    [ErrorCode(nameof(receiverId), ErrorCodes.NotFound)]
    [ErrorCode(nameof(receiverId), ErrorCodes.Duplicate, "Friend request already exists")]
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
    [ErrorCode(nameof(receiverId), ErrorCodes.NotFound)]
    [ErrorCode(nameof(receiverId), ErrorCodes.Duplicate, "Friend request already accepted")]
    public async Task<Result> AcceptFriendRequestAsync(string receiverId, string senderId)
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
    /// <param name="receiverId">Request's receiver ID</param>
    /// <param name="senderId">Request's sender ID</param>
    /// <returns></returns>
    [ErrorCode(nameof(receiverId), ErrorCodes.NotFound)]
    public async Task<Result> DeleteFriendAsync(string receiverId, string senderId)
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
    /// <param name="page">Page</param>
    /// <param name="perPage">Items per page</param>
    /// <returns>Paginated list of friend requests</returns>
    [MethodErrorCodes(typeof(Utils), nameof(Utils.PaginateAsync))]
    public async Task<Result<Pagination<FriendRequestVM>>> GetIncomingFriendRequestsAsync(string userId, int page, int perPage)
    {
        var query = context.FriendRequests
            .Where(fr => fr.ReceiverId == userId && fr.DateAccepted == null && fr.DateDeleted == null)
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
