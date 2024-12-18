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
using Reservant.Api.Identity;
using Reservant.Api.Models;
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

        if (!messageThread.IsEditable)
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

        return await query.PaginateAsync(page, perPage, []);
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

        return mapper.Map<ThreadVM>(messageThread);
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
    
    /// <summary>
    /// Creates a new thread for a report and adds appropriate participants.
    /// </summary>
    /// <param name="reportId">ID of the report</param>
    /// <param name="userId">ID of the current user</param>
    [ErrorCode(null, ErrorCodes.NotFound, "Report not found")]
    [ErrorCode(null, ErrorCodes.AccessDenied, "User not authorized to create a thread for this report")]
    [ErrorCode(null, ErrorCodes.InvalidOperation, "Report already has a thread assigned")]
    public async Task<Result<ThreadVM>> CreateThreadForReport(int reportId, Guid userId)
    {
        var report = await dbContext.Reports
            .Include(r => r.CreatedBy)
            .Include(r => r.ReportedUser)
            .FirstOrDefaultAsync(r => r.ReportId == reportId);

        if (report == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Report not found.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        if (report.ThreadId != null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Report already has a thread assigned.",
                ErrorCode = ErrorCodes.InvalidOperation
            };
        }

        var user = await dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "User not found.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        var userRoles = await userManager.GetRolesAsync(user);

        if (userRoles.Contains(Roles.Customer) || userRoles.Contains(Roles.RestaurantEmployee) || userRoles.Contains(Roles.RestaurantOwner))
        {
            // Check if the user is either CreatedBy or ReportedUser in the report
            if (report.CreatedById != userId && report.ReportedUserId != userId)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "You are not authorized to create a thread for this report.",
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }
        }
        else if (userRoles.Contains(Roles.CustomerSupportManager) || userRoles.Contains(Roles.CustomerSupportAgent))
        {
            // BOK roles can proceed
        }
        else
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "You are not authorized to create a thread for this report.",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        // Create the thread
        var threadTitle = $"Report #{report.ReportId} Discussion";
        var thread = new MessageThread
        {
            Title = threadTitle,
            CreationDate = DateTime.UtcNow,
            CreatorId = userId,
            Participants = new List<User>(),
            IsEditable = false // Wątek dla eventu nie może być edytowany
        };

        // Initialize participants list
        var participants = new List<User>();

        // Add the reporting user (CreatedBy)
        if (!participants.Any(u => u.Id == report.CreatedById))
        {
            participants.Add(report.CreatedBy);
        }

        // Add the reported user, if any
        if (report.ReportedUserId.HasValue)
        {
            var reportedUser = report.ReportedUser;
            if (reportedUser != null && !participants.Any(u => u.Id == reportedUser.Id))
            {
                participants.Add(reportedUser);
            }
        }

        // Add the current user if not already in participants
        if (!participants.Any(u => u.Id == userId))
        {
            participants.Add(user);
        }

        thread.Participants = participants;

        // Add the thread to the context
        dbContext.MessageThreads.Add(thread);

        // Assign the thread to the report
        report.Thread = thread;

        // Save changes
        await dbContext.SaveChangesAsync();

        // Map and return the thread view model
        return mapper.Map<ThreadVM>(thread);
    }


    /// <summary>
    /// Assigns the current BOK employee to the thread associated with a report.
    /// </summary>
    /// <param name="threadId">ID of the thread</param>
    /// <param name="userId">ID of the current BOK employee</param>
    [ErrorCode(null, ErrorCodes.NotFound, "Thread not found")]
    [ErrorCode(null, ErrorCodes.InvalidOperation, "Thread is not associated with any report")]
    public async Task<Result> AssignCurrentBokEmployeeToThread(int threadId, Guid userId)
    {
        var thread = await dbContext.MessageThreads
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.MessageThreadId == threadId);

        if (thread == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Thread not found.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        // Check if the thread is associated with a report
        var report = await dbContext.Reports
            .FirstOrDefaultAsync(r => r.ThreadId == threadId);

        if (report == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Thread is not associated with any report.",
                ErrorCode = ErrorCodes.InvalidOperation
            };
        }

        var user = await dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "User not found.",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        var userRoles = await userManager.GetRolesAsync(user);
        if (!userRoles.Contains(Roles.CustomerSupportManager) && !userRoles.Contains(Roles.CustomerSupportAgent))
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Only BOK employees can assign themselves to the thread.",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        if (!thread.Participants.Any(p => p.Id == userId))
        {
            thread.Participants.Add(user);
            await dbContext.SaveChangesAsync();
        }

        return Result.Success;
    }
}
