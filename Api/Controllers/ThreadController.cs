using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Message;
using Reservant.Api.Models.Dtos.Thread;
using Reservant.Api.Services;
using Reservant.Api.Validation;
using Reservant.Api.Models.Dtos;


namespace Reservant.Api.Controllers;

/// <summary>
/// Manage threads
/// </summary>
[ApiController, Route("/threads")]
public class ThreadsController(
    UserManager<User> userManager,
    ThreadService threadService
    ) : StrictController
{
    /// <summary>
    /// Create a new thread
    /// </summary>
    /// <remarks>
    /// The current user is added to the participant list automatically
    /// </remarks>
    /// <param name="request">Thread creation request</param>
    /// <returns>Created thread information</returns>
    [HttpPost]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<ThreadVM>> CreateThread([FromBody] CreateThreadRequest request)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.CreateThreadAsync(request, userId);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Update thread information
    /// </summary>
    /// <param name="threadId">ID of the thread</param>
    /// <param name="request">Thread update request</param>
    /// <returns>Updated thread information</returns>
    [HttpPut("{threadId:int}")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<ThreadVM>> UpdateThread(int threadId, [FromBody] UpdateThreadRequest request)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.UpdateThreadAsync(threadId, request, userId);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Delete a thread
    /// </summary>
    /// <param name="threadId">ID of the thread</param>
    /// <returns>Result of the deletion</returns>
    [HttpDelete("{threadId:int}")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult> DeleteThread(int threadId)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.DeleteThreadAsync(threadId, userId);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get thread information
    /// </summary>
    /// <param name="threadId">ID of the thread</param>
    /// <returns>Thread information</returns>
    [HttpGet("{threadId:int}")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<ThreadVM>> GetThread(int threadId)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.GetThreadAsync(threadId, userId);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Send message to thread
    /// </summary>
    /// <param name="threadId">ID of the thread</param>
    /// <param name="createMessageRequest">Request containing message to be passed</param>
    /// <returns>Adds message to the thread</returns>
    [HttpPost("{threadId:int}/messages")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<MessageVM>> CreateThreadsMessages(int threadId,CreateMessageRequest createMessageRequest)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.CreateThreadsMessageAsync(threadId, userId,createMessageRequest);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get threads the logged-in user participates in
    /// </summary>
    /// <remarks>
    /// Returns messages sorted by date from newest to oldest
    /// </remarks>
    /// <param name="threadId">id of thread</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Records per page</param>
    [HttpGet("{threadId:int}/messages")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<Pagination<MessageVM>>> GetThreadMessagesById(
        int threadId, [FromQuery] int page = 0, [FromQuery] int perPage = 100)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.GetThreadMessagesByIdAsync(threadId,userId, page, perPage);
        return OkOrErrors(result);
    }
}



