using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;
using Reservant.ErrorCodeDocs.Attributes;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.FriendRequests;

namespace Reservant.Api.Controllers;

/// <summary>
/// Managing friends
/// </summary>
[ApiController, Route("/friends")]
public class FriendsController(UserManager<User> userManager, FriendService service) : StrictController
{
    /// <summary>
    /// Send a friend request
    /// </summary>
    /// <param name="userId">ID of the target user</param>
    /// <response code="400">Friend request already exists</response>
    [HttpPost("{userId}/send-request")]
    [ProducesResponseType(204), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    [MethodErrorCodes<FriendService>(nameof(FriendService.SendFriendRequestAsync))]
    public async Task<ActionResult> SendFriendRequest(Guid userId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.SendFriendRequestAsync(user.Id, userId);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Mark a friend request as read
    /// </summary>
    /// <param name="senderId">ID of the user</param>
    [HttpPost("{senderId}/mark-read")]
    [ProducesResponseType(204), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    [MethodErrorCodes<FriendService>(nameof(FriendService.MarkFriendRequestAsReadAsync))]
    public async Task<ActionResult> MarkFriendRequestAsRead(Guid senderId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.MarkFriendRequestAsReadAsync(user.Id, senderId);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Accept a friend request
    /// </summary>
    /// <param name="senderId">ID of the user</param>
    [HttpPost("{senderId}/accept-request")]
    [ProducesResponseType(204), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    [MethodErrorCodes<FriendService>(nameof(FriendService.AcceptFriendRequestAsync))]
    public async Task<ActionResult> AcceptFriendRequest(Guid senderId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.AcceptFriendRequestAsync(user.Id, senderId);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Delete a friend or a peding friend request (as the sender or the receiver)
    /// </summary>
    /// <param name="otherUserId">ID of the other user</param>
    [HttpDelete("{otherUserId}")]
    [ProducesResponseType(204), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    [MethodErrorCodes<FriendService>(nameof(FriendService.DeleteFriendAsync))]
    public async Task<ActionResult> DeleteFriend(Guid otherUserId)
    {
        var result = await service.DeleteFriendAsync(otherUserId, User.GetUserId()!.Value);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get list of friends
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Records per page</param>
    /// <returns>List of friends</returns>
    [HttpGet]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    [MethodErrorCodes<FriendService>(nameof(FriendService.GetFriendsAsync))]
    public async Task<ActionResult<Pagination<FriendRequestVM>>> GetFriends([FromQuery] int page = 0,
        [FromQuery] int perPage = 10)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.GetFriendsAsync(user.Id, page, perPage);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get list of incoming friend requests
    /// </summary>
    /// <param name="unreadOnly">Return only unread requests</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Records per page</param>
    /// <returns>List of incoming friend requests</returns>
    [HttpGet("incoming")]
    [ProducesResponseType(200)]
    [Authorize(Roles = Roles.Customer)]
    [MethodErrorCodes<FriendService>(nameof(FriendService.GetIncomingFriendRequestsAsync))]
    public async Task<ActionResult<Pagination<FriendRequestVM>>> GetIncomingFriendRequests(bool unreadOnly,
        [FromQuery] int page = 0,
        [FromQuery] int perPage = 10)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.GetIncomingFriendRequestsAsync(user.Id, unreadOnly, page, perPage);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get list of outgoing friend requests
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Records per page</param>
    /// <returns>List of outgoing friend requests</returns>
    [HttpGet("outgoing")]
    [ProducesResponseType(200)]
    [Authorize(Roles = Roles.Customer)]
    [MethodErrorCodes<FriendService>(nameof(FriendService.GetOutgoingFriendRequestsAsync))]
    public async Task<ActionResult<Pagination<FriendRequestVM>>> GetOutgoingFriendRequests([FromQuery] int page = 0,
        [FromQuery] int perPage = 10)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.GetOutgoingFriendRequestsAsync(user.Id, page, perPage);
        return OkOrErrors(result);
    }
}
