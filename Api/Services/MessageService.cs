using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Message;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing messages.
/// </summary>
public class MessageService(
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
    public async Task<Result<MessageVM>> UpdateMessageAsync(int messageId, UpdateMessageRequest request, Guid userId)
    {
        var message = await dbContext.Messages
            .FirstOrDefaultAsync(m => m.MessageId == messageId);

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
            MessageId = message.MessageId,
            Contents = message.Contents,
            DateSent = message.DateSent,
            DateRead = message.DateRead,
            AuthorId = message.AuthorId,
            MessageThreadId = message.MessageThreadId
        };
    }


    /// <summary>
    /// marks message as read
    /// </summary>
    /// <param name="messageId">Id of the message</param>
    /// <param name="userId">Id of the message</param>
    /// <returns>marks message as read</returns>
    public async Task<Result<MessageVM>> MarkMessageAsReadByIdAsync(int messageId, Guid userId)
    {
        var message = await dbContext.Messages
        .Include(m => m.MessageThread)
            .ThenInclude(m => m.Participants)
        .FirstOrDefaultAsync(m => m.MessageId == messageId);

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
            MessageId = message.MessageId,
            Contents = message.Contents,
            DateSent = message.DateSent,
            DateRead = message.DateRead,
            AuthorId = message.AuthorId,
            MessageThreadId = message.MessageThreadId
        };
    }


    /// <summary>
    /// Deletes a message thread.
    /// </summary>
    /// <param name="messageId">id of a message</param>
    /// <param name="userId">ID of the user making the request</param>
    /// <returns></returns>
    public async Task<Result> DeleteMessageAsync(int messageId, Guid userId)
    {
        var message = await dbContext.Messages
            .FirstOrDefaultAsync(t => t.MessageId == messageId);

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

        return Result.Success;
    }

}
