using AutoMapper;
using AutoMapper.QueryableExtensions;
using Reservant.ErrorCodeDocs.Attributes;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Messages;
using Reservant.Api.Dtos.Threads;
using Reservant.Api.Dtos.Users;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services;

/// <summary>
/// Service for managing message threads.
/// </summary>
public class ThreadService(
    ApiDbContext dbContext,
    ValidationService validationService,
    UserManager<User> userManager,
    IMapper mapper,
    NotificationService notificationService)
{
    /// <summary>
    /// Creates a new message thread.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId">ID of the creator user</param>
    /// <returns></returns>
    public async Task<Result<ThreadVM>> CreateThreadAsync(CreateThreadRequest request, Guid userId)
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

        return mapper.Map<ThreadVM>(messageThread);
    }

    /// <summary>
    /// Updates a message thread.
    /// </summary>
    /// <param name="threadId"></param>
    /// <param name="request"></param>
    /// <param name="userId">ID of the user making the request</param>
    /// <returns></returns>
    public async Task<Result<ThreadVM>> UpdateThreadAsync(int threadId, UpdateThreadRequest request, Guid userId)
    {
        var result = await validationService.ValidateAsync(request, userId);
        if (!result.IsValid)
        {
            return result;
        }

        var messageThread = await dbContext.MessageThreads
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.MessageThreadId == threadId && t.Participants.Any(p => p.Id == userId));

        if (messageThread == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Thread not found or you are not a participant.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        if (messageThread.Type is not MessageThreadType.Normal)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "This thread cannot be edited.",
                ErrorCode = ErrorCodes.AccessDenied
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

        return mapper.Map<ThreadVM>(messageThread);
    }

    /// <summary>
    /// Deletes a message thread.
    /// </summary>
    /// <param name="threadId"></param>
    /// <param name="userId">ID of the user making the request</param>
    /// <returns></returns>
    public async Task<Result> DeleteThreadAsync(int threadId, Guid userId)
    {
        var messageThread = await dbContext.MessageThreads
            .FirstOrDefaultAsync(t => t.MessageThreadId == threadId && t.Participants.Any(p => p.Id == userId));

        if (messageThread == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Thread not found or you are not a participant.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        messageThread.IsDeleted = true;
        await dbContext.SaveChangesAsync();

        return Result.Success;
    }

    /// <summary>
    /// Gets threads for a user.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="page"></param>
    /// <param name="perPage"></param>
    /// <returns></returns>
    public async Task<Result<Pagination<ThreadVM>>> GetUserThreadsAsync(Guid userId, int page, int perPage)
    {
        var query = dbContext.MessageThreads
            .Where(t => t.Participants.Any(p => p.Id == userId))
            .ProjectTo<ThreadVM>(mapper.ConfigurationProvider);

        return await query.PaginateAsync(page, perPage, [], 100, true);
    }

    /// <summary>
    /// Gets a specific message thread for a user.
    /// </summary>
    /// <param name="threadId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<Result<ThreadVM>> GetThreadAsync(int threadId, Guid userId)
    {
        var messageThread = await dbContext.MessageThreads
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.MessageThreadId == threadId && t.Participants.Any(p => p.Id == userId));

        if (messageThread == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Thread not found or you are not a participant.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        List<UserSummaryVM> participantsMinusUs = mapper.Map<List<UserSummaryVM>>(messageThread.Participants);
        UserSummaryVM? userToRemove = participantsMinusUs.FirstOrDefault(p => p.UserId == userId);
        if (userToRemove != null)
        {
            participantsMinusUs.Remove(userToRemove);
        }

        return new ThreadVM
        {
            ThreadId = messageThread.MessageThreadId,
            Title=messageThread.Title,
            Participants = participantsMinusUs,
            Type = messageThread.Type,
        };
    }

    /// <summary>
    /// Send message to thread
    /// </summary>
    /// <param name="threadId">ID of the thread</param>
    /// <param name="userId">Request containing message to be passed</param>
    /// <param name="request">Request containing message to be passed</param>
    /// <returns>Adds message to the thread</returns>
    public async Task<Result<MessageVM>> CreateThreadsMessageAsync(int threadId, Guid userId, CreateMessageRequest request)
    {
        var messageThread = await dbContext.MessageThreads
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.MessageThreadId == threadId);

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

        var author = await dbContext.Users
            .Where(u => u.Id == userId)
            .FirstAsync();

        var message = new Message
        {
            Contents = request.Contents,
            DateSent = DateTime.UtcNow,
            AuthorId = author.Id,
            Author = author,
            MessageThreadId = threadId
        };

        var result = await validationService.ValidateAsync(message, userId);
        if (!result.IsValid)
        {
            return result;
        }

        dbContext.Add(message);
        await dbContext.SaveChangesAsync();

        await notificationService.NotifyNewMessage(
            messageThread.Participants
                .Where(participant => participant != author)
                .Select(participant => participant.Id),
            message);

        return mapper.Map<MessageVM>(message);
    }

    /// <summary>
    /// Get messages in a thread
    /// </summary>
    /// <param name="threadId">id of thread</param>
    /// <param name="userId">id of thread</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Records per page</param>
    [ErrorCode(null, ErrorCodes.NotFound)]
    [ErrorCode(null, ErrorCodes.AccessDenied)]
    [MethodErrorCodes(typeof(Utils), nameof(Utils.PaginateAsync))]
    public async Task<Result<Pagination<MessageVM>>> GetThreadMessagesByIdAsync(int threadId, Guid userId, int page, int perPage)
    {
        var messageThread = await dbContext.MessageThreads
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.MessageThreadId == threadId);

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
            .Where(m => m.MessageThreadId == threadId);

        return await query
            .OrderByDescending(m => m.DateSent)
            .ProjectTo<MessageVM>(mapper.ConfigurationProvider)
            .PaginateAsync(page, perPage, [], maxPerPage: 100);
    }

    /// <summary>
    /// Add participant to a thread
    /// </summary>
    /// <param name="threadId">ID of the thread</param>
    /// <param name="dto">DTO containing the user ID</param>
    /// <param name="currentUserId">ID of the current user for permission checks</param>
    [ErrorCode(null, ErrorCodes.NotFound, "Thread not found")]
    [ErrorCode(null, ErrorCodes.AccessDenied, "User is not a participant of the thread")]
    [ErrorCode(nameof(dto.UserId), ErrorCodes.CannotBeCurrentUser, "User cannot add themselves to a thread")]
    [ErrorCode(nameof(dto.UserId), ErrorCodes.NotFound)]
    [ErrorCode(nameof(dto.UserId), ErrorCodes.MustBeCustomerId)]
    [ErrorCode(nameof(dto.UserId), ErrorCodes.Duplicate, "User already participates in the thread")]
    public async Task<Result> AddParticipant(int threadId, AddRemoveParticipantDto dto, Guid currentUserId)
    {
        if (dto.UserId == currentUserId)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(dto.UserId),
                ErrorCode = ErrorCodes.CannotBeCurrentUser,
            };
        }

        var thread = await dbContext.MessageThreads
            .Include(t => t.Participants)
            .SingleOrDefaultAsync(t => t.MessageThreadId == threadId);
        if (thread is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = "Thread not found",
            };
        }

        if (!thread.Participants.Any(u => u.Id == currentUserId))
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.AccessDenied,
                ErrorMessage = "User is not a particpant of the thread",
            };
        }

        if (thread.Type is not MessageThreadType.Normal)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "This thread cannot be edited.",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        var targetUser = await userManager.FindByIdAsync(dto.UserId.ToString());
        if (targetUser is null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(dto.UserId),
                ErrorCode = ErrorCodes.NotFound,
            };
        }

        if (!await userManager.IsInRoleAsync(targetUser, Roles.Customer))
        {
            return new ValidationFailure
            {
                PropertyName = nameof(dto.UserId),
                ErrorCode = ErrorCodes.MustBeCustomerId,
            };
        }

        if (thread.Participants.Contains(targetUser))
        {
            return new ValidationFailure
            {
                PropertyName = nameof(dto.UserId),
                ErrorCode = ErrorCodes.Duplicate,
            };
        }

        thread.Participants.Add(targetUser);
        await dbContext.SaveChangesAsync();

        return Result.Success;
    }

    /// <summary>
    /// Remove participant from a thread
    /// </summary>
    /// <param name="threadId">ID of the thread</param>
    /// <param name="dto">DTO containing the user ID</param>
    /// <param name="currentUserId">ID of the current user for permission checks</param>
    [ErrorCode(null, ErrorCodes.NotFound, "Thread not found")]
    [ErrorCode(null, ErrorCodes.AccessDenied, "User is not a participant of the thread")]
    [ErrorCode(nameof(dto.UserId), ErrorCodes.NotFound)]
    public async Task<Result> RemoveParticipant(int threadId, AddRemoveParticipantDto dto, Guid currentUserId)
    {
        var thread = await dbContext.MessageThreads
            .Include(t => t.Participants)
            .SingleOrDefaultAsync(t => t.MessageThreadId == threadId);
        if (thread is null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = "Thread not found",
            };
        }

        if (!thread.Participants.Any(u => u.Id == currentUserId))
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorCode = ErrorCodes.AccessDenied,
                ErrorMessage = "User is not a particpant of the thread",
            };
        }

        if (thread.Type is not MessageThreadType.Normal)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "This thread cannot be edited.",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        var targetUser = thread.Participants.SingleOrDefault(p => p.Id == dto.UserId);
        if (targetUser is null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(dto.UserId),
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = "Participant not found",
            };
        }

        thread.Participants.Remove(targetUser);
        if (thread.Participants.Count == 0)
        {
            thread.IsDeleted = true;
        }

        await dbContext.SaveChangesAsync();

        return Result.Success;
    }
}
