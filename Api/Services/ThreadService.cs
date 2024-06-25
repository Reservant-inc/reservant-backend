using System.Security.Claims;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
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
            Participants = participants
        };

        var validationResult = await validationService.ValidateAsync(messageThread, null);
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

        var validationResult = await validationService.ValidateAsync(messageThread, null);
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

        return await query.PaginateAsync(page, perPage);
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
}
