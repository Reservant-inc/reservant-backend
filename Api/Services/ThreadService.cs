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

    /// <summary>
    /// adds message to thread from provided id, provided user is its participant
    /// </summary>
    /// <param name="threadId">ID of the thread</param>
    /// <param name="userId">Request containing message to be passed</param>
    /// <param name="request">Request containing message to be passed</param>
    /// <returns>Adds message to the thread</returns>
    public async Task<Result<MessageVM>> CreateThreadsMessageAsync(int threadId, string userId, CreateMessageRequest request)
    {
        var messageThread = await dbContext.MessageThreads
            .Include(t => t.Participants)
            .Include(t => t.Messages)
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

        var result = await validationService.ValidateAsync(message, userId);
        if (!result.IsValid)
        {
            return result;
        }

        var user = await dbContext.Users
        .Where(u => u.Id == userId)
        .FirstOrDefaultAsync();

        if (messageThread.Messages == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "ITS A NULL, BIG SUPRISE",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }


        messageThread.Messages.Add(message);
        await dbContext.SaveChangesAsync();

        return new MessageVM
        {
            MessageId = message.Id,
            Contents = message.Contents,
            DateSent = message.DateSent,
            DateRead = message.DateRead,
            AuthorsFirstName = user.FirstName,
            AuthorsLastName = user.LastName,
            MessageThreadId = message.MessageThreadId
        };
    }

    /// <summary>
    /// Get threads the logged-in user participates in
    /// </summary>
    /// <param name="threadId">id of thread</param>
    /// <param name="userId">id of thread</param>
    /// <param name="messageId">id of a message to dispaly</param>
    /// <param name="perPage">Records per page</param>
    /// <returns>returns paginated messages starting with provided message id </returns>
   
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
            .Include(m => m.Author)
            .Where(m => m.MessageThreadId == threadId && m.Id > messageId)
            .OrderByDescending(m => m.DateSent)
            .Select(m => new MessageVM
            {
                MessageId = m.Id,
                Contents = m.Contents,
                DateSent = m.DateSent,
                DateRead = m.DateRead,
                AuthorsFirstName = m.Author.FirstName,
                AuthorsLastName = m.Author.LastName,
                MessageThreadId = m.MessageThreadId
            });

        return await query.PaginateAsync(0, perPage, []);
    }
}
