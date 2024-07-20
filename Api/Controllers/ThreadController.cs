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
    ) : ControllerBase
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
    /// adds message to thread from provided id, provided user is its participant
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


    //COŚ DZIWNIE ZWRACA
    /// <summary>
    /// Get threads the logged-in user participates in
    /// </summary>
    /// <param name="threadId">id of thread</param>
    /// <param name="messageId">id of a message to dispaly</param>
    /// <param name="perPage">Records per page</param>
    /// <returns>returns paginated messages starting with provided message id </returns>
    [HttpGet("{threadId:int}/messticipatesages")]
    [Authorize(Roles = Roles.Customer)]
    [ProducesResponseType(200), ProducesResponseType(401)]
    public async Task<ActionResult<Pagination<MessageVM>>> GetThreadMessagesById(int threadId,int messageId, [FromQuery] int perPage = 10)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.GetThreadMessagesByIdAsync(threadId,userId, messageId, perPage);
        // if (result.IsError)
        // {
        //     return result.ToValidationProblem();
        // } //IDK IT KILLS THE BUILD, WORKS NORMAL FOR ThreadSummaryVM

        return Ok(result.Value);
    }







    //WYWALIĆ DO KONTROLERA POTEM, NIE ZAPOMINIJ USUNNĄ /messages

    /// <summary>
    /// Edit message
    /// </summary>
    /// <param name="messageId">ID of the message</param>
    /// <param name="request">Message update request</param>
    /// <returns>Updated message information</returns>
    [HttpPut("messages/{messageId:int}")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<MessageVM>> UpdateMessage(int messageId, [FromBody] UpdateMessageRequest request)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.UpdateMessageAsync(messageId, request, userId);
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
    }






    /// <summary>
    /// marks message as read
    /// </summary>
    /// <param name="messageId">ID of the message</param>
    /// <returns>marks message as read</returns>
    [HttpPost("messages/{messageId:int}/mark-read")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<MessageVM>> MarkMessageAsReadById(int messageId)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.MarkMessageAsReadByIdAsync(messageId, userId);
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
    }




    /// <summary>
    /// Delete a message
    /// </summary>
    /// <param name="messageId">ID of the message</param>
    /// <returns>Result of the deletion</returns>
    [HttpDelete("messages/{messageId:int}")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult> DeleteMessage(int messageId)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await threadService.DeleteMessageAsync(messageId, userId);
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok();
    }

}



