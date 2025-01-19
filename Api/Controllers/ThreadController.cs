using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Services;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Messages;
using Reservant.Api.Dtos.Threads;
using Reservant.ErrorCodeDocs.Attributes;


namespace Reservant.Api.Controllers;

/// <summary>
/// Manage threads
/// </summary>
[ApiController, Route("/threads")]
public class ThreadsController(
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
    [AuthorizeRoles(Roles.Customer, Roles.CustomerSupportAgent)]
    public async Task<ActionResult<ThreadVM>> CreateThread([FromBody] CreateThreadRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.CreateThreadAsync(request, userId.Value);
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
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.UpdateThreadAsync(threadId, request, userId.Value);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Delete a thread
    /// </summary>
    /// <param name="threadId">ID of the thread</param>
    /// <returns>Result of the deletion</returns>
    [HttpDelete("{threadId:int}")]
    [ProducesResponseType(204), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult> DeleteThread(int threadId)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.DeleteThreadAsync(threadId, userId.Value);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get thread information
    /// </summary>
    /// <param name="threadId">ID of the thread</param>
    /// <returns>Thread information</returns>
    [HttpGet("{threadId:int}")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [AuthorizeRoles(Roles.Customer, Roles.CustomerSupportAgent)]
    public async Task<ActionResult<ThreadVM>> GetThread(int threadId)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.GetThreadAsync(threadId, userId.Value);
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
    [AuthorizeRoles(Roles.Customer, Roles.CustomerSupportAgent)]
    public async Task<ActionResult<MessageVM>> CreateThreadsMessages(int threadId,CreateMessageRequest createMessageRequest)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.CreateThreadsMessageAsync(threadId, userId.Value, createMessageRequest);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get messages in a thread
    /// </summary>
    /// <remarks>
    /// Use `GET /threads/{threadId}` first to fetch and cache the participants info.
    /// Then you can use `authorId` to get information about the message author locally.
    ///
    /// Returns messages sorted by date from newest to oldest
    /// </remarks>
    /// <param name="threadId">id of thread</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Records per page</param>
    [HttpGet("{threadId:int}/messages")]
    [AuthorizeRoles(Roles.Customer, Roles.CustomerSupportAgent)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<ThreadService>(nameof(ThreadService.GetThreadMessagesByIdAsync))]
    public async Task<ActionResult<Pagination<MessageVM>>> GetThreadMessagesById(
        int threadId, [FromQuery] int page = 0, [FromQuery] int perPage = 100)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.GetThreadMessagesByIdAsync(threadId,userId.Value, page, perPage);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Add participant to a thread
    /// </summary>
    /// <param name="threadId">ID of the thread</param>
    /// <param name="dto">DTO containing the user ID</param>
    [HttpPost("{threadId:int}/add-participant")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(204), ProducesResponseType(400)]
    [MethodErrorCodes<ThreadService>(nameof(ThreadService.AddParticipant))]
    public async Task<ActionResult> AddParticipant(int threadId, AddRemoveParticipantDto dto)
    {
        return OkOrErrors(await threadService.AddParticipant(
            threadId, dto, User.GetUserId()!.Value));
    }

    /// <summary>
    /// Remove participant from a thread
    /// </summary>
    /// <remarks>
    /// Can also be used to leave the thread
    /// </remarks>
    /// <param name="threadId">ID of the thread</param>
    /// <param name="dto">DTO containing the user ID</param>
    [HttpPost("{threadId:int}/remove-participant")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(204), ProducesResponseType(400)]
    [MethodErrorCodes<ThreadService>(nameof(ThreadService.RemoveParticipant))]
    public async Task<ActionResult> RemoveParticipant(int threadId, AddRemoveParticipantDto dto)
    {
        return OkOrErrors(await threadService.RemoveParticipant(
            threadId, dto, User.GetUserId()!.Value));
    }

    /// <summary>
    /// Create a new private thread chat between 2 users
    /// </summary>
    [HttpPost("create-private-thread")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<ThreadService>(nameof(ThreadService.CreatePrivateThreadAsync))]
    public async Task<ActionResult<ThreadVM>> CreatePrivateThread(CreatePrivateThreadRequest dto)
    {
        var currentUserId = User.GetUserId();
        if(currentUserId == null)
        {
            return Unauthorized();
        }
        return OkOrErrors(await threadService.CreatePrivateThreadAsync((Guid)currentUserId, dto.OtherUserId));
    }
}
