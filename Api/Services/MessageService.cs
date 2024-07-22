using System.Security.Claims;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Reservant.Api.Data;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.Message;
using Reservant.Api.Models.Dtos.Thread;
using Reservant.Api.Models.Dtos.User;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing messages.
/// </summary>
public class MessageService(
    UserManager<User> userManager,
    ApiDbContext dbContext,
    ValidationService validationService)
{
     /// <summary>
    /// Updates a message by Id.
    /// </summary>
    /// <param name="messageId"> id of a messfe</param>
    /// <param name="request"> update request</param>
    /// <param name="userId">ID of the user making the request</param>
    /// <returns></returns>
    public async Task<Result<MessageVM>> UpdateMessageAsync(int messageId, UpdateMessageRequest request, string userId)
    {
        var message = await dbContext.Messages
        .Include(m => m.Author)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "message not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        if (message.AuthorId!=userId)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "message was not send by provided user",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        message.Contents = request.Contents;
    
        var validationResult = await validationService.ValidateAsync(message, userId);
        if (!validationResult.IsValid)
        {
            return validationResult;
        }

        dbContext.Messages.Update(message);
        await dbContext.SaveChangesAsync();

        return new MessageVM
        {
            MessageId = message.Id,
            Contents = message.Contents,
            DateSent = message.DateSent,
            DateRead = message.DateRead,
            AuthorsFirstName = message.Author.FirstName,
            AuthorsLastName = message.Author.LastName,
            MessageThreadId = message.MessageThreadId
        };
    }


    /// <summary>
    /// marks message as read
    /// </summary>
    /// <param name="messageId">Id of the message</param>
    /// <param name="userId">Id of the message</param>
    /// <returns>marks message as read</returns>
    public async Task<Result<MessageVM>> MarkMessageAsReadByIdAsync(int messageId, string userId)
    {
        var message = await dbContext.Messages
        .Include(m => m.Author)
        .Include(m => m.MessageThread)
            .ThenInclude(m => m.Participants)
        .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Message not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        
        if (message.DateRead == null)
        {
            if (!message.MessageThread.Participants.Any(p => p.Id == userId))
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "Thread not accasable to provided user.",
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

            if (message.MessageThread.CreatorId==userId)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "Message created by the same user can not be read by the same user",
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

            message.DateRead = DateTime.UtcNow;
        
            var validationResult = await validationService.ValidateAsync(message, userId);
            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            dbContext.Messages.Update(message);
            await dbContext.SaveChangesAsync();
        }

        return new MessageVM
        {
            MessageId = message.Id,
            Contents = message.Contents,
            DateSent = message.DateSent,
            DateRead = message.DateRead,
            AuthorsFirstName = message.Author.FirstName,
            AuthorsLastName = message.Author.LastName,
            MessageThreadId = message.MessageThreadId
        };   
    }


    /// <summary>
    /// Deletes a message thread.
    /// </summary>
    /// <param name="messageId">id of a message</param>
    /// <param name="userId">ID of the user making the request</param>
    /// <returns></returns>
    public async Task<Result<bool>> DeleteMessageAsync(int messageId, string userId)
    {
        var message = await dbContext.Messages
            .FirstOrDefaultAsync(t => t.Id == messageId);

        if (message == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Message not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        if (message.AuthorId != userId) 
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Provided user is not the author",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        dbContext.Messages.Remove(message);
        await dbContext.SaveChangesAsync();

        return true;
    }

}
