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
/// Service for managing message threads.
/// </summary>
public class ThreadService(
    UserManager<User> userManager,
    ApiDbContext dbContext,
    ValidationService validationService)
{
    /// <summary>
    /// Creates a new message thread.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId">ID of the creator user</param>
    /// <returns></returns>
    public async Task<Result<ThreadVM>> CreateThreadAsync(CreateThreadRequest request, string userId)
    {
        var result = await validationService.ValidateAsync(request, userId);
        if (!result.IsValid)
        {
            return result;
        }

        var participants = await dbContext.Users
            .Where(u => request.ParticipantIds.Contains(u.Id))
            .ToListAsync();

        if (participants.Count != request.ParticipantIds.Count)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(request.ParticipantIds),
                ErrorMessage = "One or more participants do not exist.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        // Add creator to the participants list
        var creator = await dbContext.Users.FindAsync(userId);
        if (creator != null && !participants.Any(p => p.Id == userId))
        {
            participants.Add(creator);
        }

        var messageThread = new MessageThread
        {
            Title = request.Title,
            CreationDate = DateTime.UtcNow,
            CreatorId = userId,
            Participants = participants,
            //Messages = new List<Message>() //NIE DZIAŁĄ
        };

        var validationResult = await validationService.ValidateAsync(messageThread, userId);
        if (!validationResult.IsValid)
        {
            return validationResult;
        }

        dbContext.MessageThreads.Add(messageThread);
        await dbContext.SaveChangesAsync();

        return new ThreadVM
        {
            ThreadId = messageThread.Id,
            Title = messageThread.Title,
            Participants = messageThread.Participants.Select(p => new UserSummaryVM { UserId = p.Id, FirstName = p.FirstName, LastName = p.LastName }).ToList()
        };
    }

    /// <summary>
    /// Updates a message thread.
    /// </summary>
    /// <param name="threadId"></param>
    /// <param name="request"></param>
    /// <param name="userId">ID of the user making the request</param>
    /// <returns></returns>
    public async Task<Result<ThreadVM>> UpdateThreadAsync(int threadId, UpdateThreadRequest request, string userId)
    {
        var result = await validationService.ValidateAsync(request, userId);
        if (!result.IsValid)
        {
            return result;
        }

        var messageThread = await dbContext.MessageThreads
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.Id == threadId && t.Participants.Any(p => p.Id == userId));

        if (messageThread == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Thread not found or you are not a participant.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        messageThread.Title = request.Title;

        var validationResult = await validationService.ValidateAsync(messageThread, userId);
        if (!validationResult.IsValid)
        {
            return validationResult;
        }

        dbContext.MessageThreads.Update(messageThread);
        await dbContext.SaveChangesAsync();

        return new ThreadVM
        {
            ThreadId = messageThread.Id,
            Title = messageThread.Title,
            Participants = messageThread.Participants.Select(p => new UserSummaryVM { UserId = p.Id, FirstName = p.FirstName, LastName = p.LastName }).ToList()
        };
    }

    /// <summary>
    /// Deletes a message thread.
    /// </summary>
    /// <param name="threadId"></param>
    /// <param name="userId">ID of the user making the request</param>
    /// <returns></returns>
    public async Task<Result<bool>> DeleteThreadAsync(int threadId, string userId)
    {
        var messageThread = await dbContext.MessageThreads
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.Id == threadId && t.Participants.Any(p => p.Id == userId));

        if (messageThread == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Thread not found or you are not a participant.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        dbContext.MessageThreads.Remove(messageThread);
        await dbContext.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Gets threads for a user.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="page"></param>
    /// <param name="perPage"></param>
    /// <returns></returns>
    public async Task<Result<Pagination<ThreadSummaryVM>>> GetUserThreadsAsync(string userId, int page, int perPage)
    {
        var query = dbContext.MessageThreads
            .Where(t => t.Participants.Any(p => p.Id == userId))
            .Select(t => new ThreadSummaryVM
            {
                Title = t.Title,
                NumberOfParticipants = t.Participants.Count
            });

        return await query.PaginateAsync(page, perPage, []);
    }

    /// <summary>
    /// Gets a specific message thread for a user.
    /// </summary>
    /// <param name="threadId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<Result<ThreadVM>> GetThreadAsync(int threadId, string userId)
    {
        var messageThread = await dbContext.MessageThreads
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.Id == threadId && t.Participants.Any(p => p.Id == userId));

        if (messageThread == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Thread not found or you are not a participant.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        return new ThreadVM
        {
            ThreadId = messageThread.Id,
            Title = messageThread.Title,
            Participants = messageThread.Participants.Select(p => new UserSummaryVM { UserId = p.Id, FirstName = p.FirstName, LastName = p.LastName }).ToList()
        };
    }












    public async Task<Result<MessageVM>> CreateThreadsMessageAsync(int threadId, string userId, CreateMessageRequest request)
    {
        //PROBLEM, ROBIE W POPRZEDNIEJ SESJII, W TEJ JUŻ NIE ŻYJE
        var messageThread = await dbContext.MessageThreads
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (messageThread == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Thread not found or you are not a participant.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        if (!messageThread.Participants.Any(p => p.Id == userId))
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Thread not accasable to provided user.",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }


        var message = new Message
        {
            Contents = request.Contents,
            DateSent = DateTime.UtcNow,
            AuthorId = userId,
            MessageThreadId = threadId
        };

        //CZEMU JA TO MUSZĘ ROBIĆ?!?!?!
        if (messageThread.Messages==null)
        {
            messageThread.Messages = new List<Message>();
        }

        messageThread.Messages.Add(message);
        await dbContext.SaveChangesAsync();

        messageThread.Messages.Add(message);

        return new MessageVM
        {
            Id = message.Id,
            Contents = message.Contents,
            DateSent = message.DateSent,
            DateRead = message.DateRead,
            AuthorId = message.AuthorId,
            MessageThreadId = message.MessageThreadId
        };
    }



    public async Task<Result<Pagination<MessageVM>>> GetThreadMessagesByIdAsync(int threadId, String userId, int messageId, int perPage)
    {
        var messageThread = await dbContext.MessageThreads
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (messageThread == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Thread not found or you are not a participant.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        if (!messageThread.Participants.Any(p => p.Id == userId))
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Thread not accasable to provided user.",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        var query = dbContext.Messages
                .Where(t => t.MessageThreadId == threadId)
                .OrderByDescending(t => t.DateSent)
                .Select(t => new MessageVM
                {
                    Id = t.Id,
                    Contents = t.Contents,
                    DateSent = t.DateSent,
                    DateRead = t.DateRead,
                    AuthorId = t.AuthorId,
                    MessageThreadId = t.MessageThreadId
                });

        if (query.Count() == 0)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "No messages of provided thread found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        return await query.PaginateAsync(messageId, perPage, []);
    }













    //wyeksportować do thread services kiedy zaczną działać


    /// <summary>
    /// Updates a message by Id.
    /// </summary>
    /// <param name="messaged"></param>
    /// <param name="request"></param>
    /// <param name="userId">ID of the user making the request</param>
    /// <returns></returns>
    public async Task<Result<MessageVM>> UpdateMessageAsync(int messageId, UpdateMessageRequest request, string userId)
    {
        // var result = await validationService.ValidateAsync(request, userId);
        // if (!result.IsValid)
        // {
        //     return result;
        // }

        var message = await dbContext.Messages
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
    
        // var validationResult = await validationService.ValidateAsync(message, userId);
        // if (!validationResult.IsValid)
        // {
        //     return validationResult;
        // }

        dbContext.Messages.Update(message);
        await dbContext.SaveChangesAsync();

        return new MessageVM
        {
            Id = message.Id,
            Contents = message.Contents,
            DateSent = message.DateSent,
            DateRead = message.DateRead,
            AuthorId = message.AuthorId,
            MessageThreadId = message.MessageThreadId
        };
        //CANT CHECK IF IT WORKS, RETURNS AS IF IT DID
    
    }



    public async Task<Result<MessageVM>> MarkMessageAsReadByIdAsync(int mesageId, string userId)
    {
        var message = await dbContext.Messages
        .Include(m => m.MessageThread)
            .ThenInclude(m => m.Participants)
        .FirstOrDefaultAsync(m => m.Id == mesageId);

        if (message == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Message not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

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
    
        // var validationResult = await validationService.ValidateAsync(message, userId);
        // if (!validationResult.IsValid)
        // {
        //     return validationResult;
        // }

        dbContext.Messages.Update(message);
        await dbContext.SaveChangesAsync();

        return new MessageVM
        {
            Id = message.Id,
            Contents = message.Contents,
            DateSent = message.DateSent,
            DateRead = message.DateRead,
            AuthorId = message.AuthorId,
            MessageThreadId = message.MessageThreadId
        };
        //CANT CHECK IF IT WORKS, RETURNS AS IF IT DID
    }


    /// <summary>
    /// Deletes a message thread.
    /// </summary>
    /// <param name="messageId"></param>
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
