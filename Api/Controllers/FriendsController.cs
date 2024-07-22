using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.FriendRequest;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Services;
using Reservant.Api.Validation;
using ErrorCodeDocs;

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
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    [MethodErrorCodes(nameof(FriendService.SendFriendRequestAsync))]
    public async Task<ActionResult> SendFriendRequest(string userId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.SendFriendRequestAsync(user.Id, userId);

        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok();
    }

    /// <summary>
    /// Mark a friend request as read
    /// </summary>
    /// <param name="senderId">ID of the user</param>
    [HttpPost("{senderId}/mark-read")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    [MethodErrorCodes(nameof(FriendService.MarkFriendRequestAsReadAsync))]
    public async Task<ActionResult> MarkFriendRequestAsRead(string senderId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.MarkFriendRequestAsReadAsync(user.Id, senderId);

        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok();
    }

    /// <summary>
    /// Accept a friend request
    /// </summary>
    /// <param name="senderId">ID of the user</param>
    [HttpPost("{senderId}/accept-request")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    [MethodErrorCodes(nameof(FriendService.AcceptFriendRequestAsync))]
    public async Task<ActionResult> AcceptFriendRequest(string senderId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.AcceptFriendRequestAsync(user.Id, senderId);

        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok();
    }

    /// <summary>
    /// Delete a friend or a peding friend request (as the sender or the receiver)
    /// </summary>
    /// <param name="userId">ID of the user</param>
    [HttpDelete("{userId}")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    [MethodErrorCodes(nameof(FriendService.DeleteFriendAsync))]
    public async Task<ActionResult> DeleteFriend(string userId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.DeleteFriendAsync(user.Id, userId);

        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok();
    }

    /// <summary>
    /// Get list of friends
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Records per page</param>
    /// <returns>List of friends</returns>
    [HttpGet]
    [ProducesResponseType(200)]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<Pagination<FriendRequestVM>>> GetFriends([FromQuery] int page = 0,
        [FromQuery] int perPage = 10)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.GetFriendsAsync(user.Id, page, perPage);

        return Ok(result.Value);
    }

    /// <summary>
    /// Get list of incoming friend requests
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Records per page</param>
    /// <returns>List of incoming friend requests</returns>
    [HttpGet("incoming")]
    [ProducesResponseType(200)]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<Pagination<FriendRequestVM>>> GetIncomingFriendRequests([FromQuery] int page = 0,
        [FromQuery] int perPage = 10)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.GetIncomingFriendRequestsAsync(user.Id, page, perPage);

        return Ok(result.Value);
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
    public async Task<ActionResult<Pagination<FriendRequestVM>>> GetOutgoingFriendRequests([FromQuery] int page = 0,
        [FromQuery] int perPage = 10)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.GetOutgoingFriendRequestsAsync(user.Id, page, perPage);

        return Ok(result.Value);
    }
}
