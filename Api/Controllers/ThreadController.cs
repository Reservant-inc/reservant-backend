﻿using Microsoft.AspNetCore.Authorization;
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
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
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
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
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
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok();
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
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
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
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get threads the logged-in user participates in
    /// </summary>
    /// <param name="threadId">id of thread</param>
    /// <param name="returnBefore">Return messages before (&lt;) the time. Used for pagination</param>
    /// <param name="perPage">Records per page</param>
    /// <returns>returns paginated messages starting with provided message id </returns>
    [HttpGet("{threadId:int}/messages")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult<List<MessageVM>>> GetThreadMessagesById(int threadId, DateTime? returnBefore = null, [FromQuery] int perPage = 100)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.GetThreadMessagesByIdAsync(threadId,userId, returnBefore, perPage);
        if (result.IsError)
        {
            return result.ToValidationProblem();
        } 

        return Ok(result.Value);
    }
}



