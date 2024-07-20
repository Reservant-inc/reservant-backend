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
    // /// <summary>
    // /// Updates a message by Id.
    // /// </summary>
    // /// <param name="messaged"></param>
    // /// <param name="request"></param>
    // /// <param name="userId">ID of the user making the request</param>
    // /// <returns></returns>
    // public async Task<Result<MessageVM>> UpdateMessageAsync(int messageId, UpdateMessageRequest request, string userId)
    // {
    //     var result = await validationService.ValidateAsync(request, userId);
    //     if (!result.IsValid)
    //     {
    //         return result;
    //     }

    //     var message = await dbContext.Messages
    //         .FirstOrDefaultAsync(m => m.Id == messageId);

    //     if (message == null)
    //     {
    //         return new ValidationFailure
    //         {
    //             PropertyName = null,
    //             ErrorMessage = "message not found",
    //             ErrorCode = ErrorCodes.NotFound
    //         };
    //     }

    //     if (message.AuthorId!=userId)
    //     {
    //         return new ValidationFailure
    //         {
    //             PropertyName = null,
    //             ErrorMessage = "message was not send by provided user",
    //             ErrorCode = ErrorCodes.AccessDenied
    //         };
    //     }

    //     message.Contents = request.Contents;
    
    //     var validationResult = await validationService.ValidateAsync(message, userId);
    //     if (!validationResult.IsValid)
    //     {
    //         return validationResult;
    //     }

    //     dbContext.Messages.Update(message);
    //     await dbContext.SaveChangesAsync();

    //     return new MessageVM
    //     {
    //         Id = message.Id,
    //         Contents = message.Contents,
    //         DateSent = message.DateSent,
    //         DateRead = message.DateRead,
    //         AuthorId = message.AuthorId,
    //         MessageThreadId = message.MessageThreadId
    //     };
    // }

}
