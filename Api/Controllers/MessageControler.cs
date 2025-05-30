using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Dtos.Messages;
using Reservant.Api.Validators;


namespace Reservant.Api.Controllers;

/// <summary>
/// Manage threads
/// </summary>
[ApiController, Route("/messages")]
public class MessageController(
    MessageService messageService
    ) : StrictController
{
    /// <summary>
    /// Edit message
    /// </summary>
    /// <param name="messageId">ID of the message</param>
    /// <param name="request">Message update request</param>
    /// <returns>Updated message information</returns>
    [HttpPut("{messageId:int}")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<MessageVM>> UpdateMessage(int messageId, [FromBody] UpdateMessageRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await messageService.UpdateMessageAsync(messageId, request, userId.Value);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Marks message as read
    /// </summary>
    /// <param name="messageId">ID of the message</param>
    /// <returns>marks message as read</returns>
    [HttpPost("{messageId:int}/mark-read")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<MessageVM>> MarkMessageAsReadById(int messageId)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await messageService.MarkMessageAsReadByIdAsync(messageId, userId.Value);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Delete a message
    /// </summary>
    /// <param name="messageId">ID of the message</param>
    /// <returns>Result of the deletion</returns>
    [HttpDelete("{messageId:int}")]
    [ProducesResponseType(204), ProducesResponseType(400)]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult> DeleteMessage(int messageId)
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await messageService.DeleteMessageAsync(messageId, userId.Value);
        return OkOrErrors(result);
    }

}



