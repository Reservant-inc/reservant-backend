﻿using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Event;
using Reservant.Api.Models.Dtos.User;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services
{
    /// <summary>
    /// Service for event management
    /// </summary>
    public class EventService(
        ApiDbContext context,
        ValidationService validationService,
        FileUploadService uploadService)
    {
        /// <summary>
        /// Action for
        /// </summary>
        /// <param name="request"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [ValidatorErrorCodes<CreateEventRequest>]
        [ErrorCode(nameof(CreateEventRequest.RestaurantId), ErrorCodes.NotFound)]
        [ValidatorErrorCodes<Event>]
        public async Task<Result<EventVM>> CreateEventAsync(CreateEventRequest request, User user)
        {
            var result = await validationService.ValidateAsync(request, user.Id);
            if (!result.IsValid) {
                return result;
            }
            var restaurant = await context.Restaurants.FindAsync(request.RestaurantId);
            if (restaurant is null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(request.RestaurantId),
                    ErrorCode = ErrorCodes.NotFound,
                    ErrorMessage = ErrorCodes.NotFound
                };
            }
            var newEvent = new Event
            {
                CreatedAt = DateTime.UtcNow,
                Description = request.Description,
                Time = request.Time,
                MaxPeople = request.MaxPeople,
                MustJoinUntil = request.MustJoinUntil,
                CreatorId = user.Id,
                RestaurantId = request.RestaurantId,
                Creator = user,
                Restaurant = restaurant,
                Interested = new List<User>(),
                IsDeleted = false
            };
            result = await validationService.ValidateAsync(newEvent, user.Id);
            if (!result.IsValid)
            {
                return result;
            }
            await context.Events.AddAsync(newEvent);
            await context.SaveChangesAsync();

            return new EventVM
            {
                CreatedAt = newEvent.CreatedAt,
                Description = newEvent.Description,
                Time = newEvent.Time,
                MaxPeople = newEvent.MaxPeople,
                EventId = newEvent.Id,
                MustJoinUntil = newEvent.MustJoinUntil,
                CreatorId = newEvent.CreatorId,
                CreatorFullName = user.FullName,
                RestaurantId = newEvent.RestaurantId,
                RestaurantName = newEvent.Restaurant.Name,
                VisitId = newEvent.VisitId,
                Interested = newEvent.Interested.Select(i => new UserSummaryVM
                {
                    FirstName = i.FirstName,
                    LastName = i.LastName,
                    UserId = i.Id,
                    Photo = uploadService.GetPathForFileName(i.PhotoFileName),
                }).ToList()
            };
        }
        
        /// <summary>
        /// Add user from event's interested list
        /// </summary>
        [ErrorCode(null, ErrorCodes.NotFound)]
        [ErrorCode(null, ErrorCodes.Duplicate, "User is already interested in the event")]
        public async Task<Result> AddUserToEventAsync(int id, User user)
        {
            var eventFound = await context.Events
                .Include(e => e.Interested)
                .Where(e=>e.Id==id)
                .FirstOrDefaultAsync();
            if(eventFound==null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "Event not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            if (eventFound.Interested.Contains(user))
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "User is already interested in the event",
                    ErrorCode = ErrorCodes.Duplicate
                };
            }

            eventFound.Interested.Add(user);
            await context.SaveChangesAsync();

            return Result.Success;
        }

        /// <summary>
        /// Get information about an Event
        /// </summary>
        [ErrorCode(null, ErrorCodes.NotFound)]
        public async Task<Result<EventVM>> GetEventAsync(int id)
        {
            var checkedEvent = await context.Events
                .Include(e => e.Interested)
                .Include(e => e.Creator)
                .Include(e => e.Restaurant)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (checkedEvent is null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.NotFound,
                    ErrorMessage = ErrorCodes.NotFound
                };
            }
            return new EventVM
            {
                CreatedAt = checkedEvent.CreatedAt,
                Description = checkedEvent.Description,
                Time = checkedEvent.Time,
                MaxPeople = checkedEvent.MaxPeople,
                EventId = checkedEvent.Id,
                MustJoinUntil = checkedEvent.MustJoinUntil,
                CreatorId = checkedEvent.CreatorId,
                CreatorFullName = checkedEvent.Creator.FullName,
                RestaurantId = checkedEvent.RestaurantId,
                RestaurantName = checkedEvent.Restaurant.Name,
                VisitId = checkedEvent.VisitId,
                Interested = checkedEvent.Interested.Select(i => new UserSummaryVM
                {
                    FirstName = i.FirstName,
                    LastName = i.LastName,
                    UserId = i.Id,
                    Photo = uploadService.GetPathForFileName(i.PhotoFileName),
                }).ToList()
            };
        }

        /// <summary>
        /// Get events created by the given User
        /// </summary>
        public async Task<Result<List<EventSummaryVM>>> GetEventsCreatedAsync(User user)
        {
            var events = await context.Events
                .Include(e => e.Interested)
                .Include(e => e.Creator)
                .Include(e => e.Restaurant)
                .Where(e => e.CreatorId == user.Id)
                .ToListAsync();

            return events.Select(e => new EventSummaryVM
            {
                CreatorFullName = e.Creator.FullName,
                Time = e.Time,
                MaxPeople = e.MaxPeople,
                RestaurantName = e.Restaurant.Name,
                RestaurantId = e.Restaurant.Id,
                NumberInterested = e.Interested.Count,
                MustJoinUntil = e.MustJoinUntil,
                EventId = e.Id,
                Description = e.Description,
                CreatorId = e.CreatorId,
            }).ToList();
        }

        /// <summary>
        /// Accept a user to an event
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <param name="userToAcceptId">ID of the user to be accepted</param>
        /// <param name="currentUser">Current user (event creator)</param>
        /// <returns></returns>
        [ErrorCode(null, ErrorCodes.NotFound, "Event not found")]
        [ErrorCode(null, ErrorCodes.AccessDenied, "Only the event creator can accept users")]
        [ErrorCode(null, ErrorCodes.UserNotInterestedInEvent, "User is not interested in the event")]
        [ErrorCode(null, ErrorCodes.UserAlreadyAccepted, "User is already accepted to the event")]
        [ErrorCode(null, ErrorCodes.EventIsFull, "The event has reached its maximum number of participants")]
        [ErrorCode(null, ErrorCodes.JoinDeadlinePassed, "The deadline to join this event has passed")]
        public async Task<Result> AcceptUserToEventAsync(int eventId, string userToAcceptId, User currentUser)
        {
            var eventFound = await context.Events
                .Include(e => e.Interested)
                .Include(e => e.Creator)
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == eventId && !e.IsDeleted);

            if (eventFound == null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "Event not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            if (eventFound.CreatorId != currentUser.Id)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "Only the event creator can accept users",
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

            var userToAccept = await context.Users.FindAsync(userToAcceptId);
            if (userToAccept == null || eventFound.Interested.All(u => u.Id != userToAcceptId))
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "User is not interested in the event",
                    ErrorCode = ErrorCodes.UserNotInterestedInEvent
                };
            }

            if (eventFound.Participants.Any(u => u.Id == userToAcceptId))
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "User is already accepted to the event",
                    ErrorCode = ErrorCodes.UserAlreadyAccepted
                };
            }

            if (eventFound.Participants.Count >= eventFound.MaxPeople)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "The event has reached its maximum number of participants",
                    ErrorCode = ErrorCodes.EventIsFull
                };
            }

            if (DateTime.UtcNow > eventFound.MustJoinUntil)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "The deadline to join this event has passed",
                    ErrorCode = ErrorCodes.JoinDeadlinePassed
                };
            }

            var existingInvite = await context.EventInviteRequests
                .FirstOrDefaultAsync(ei => ei.Event.Id == eventId && ei.ReceiverId == userToAcceptId && !ei.IsDeleted);

            if (existingInvite != null)
            {
                existingInvite.DateAccepted = DateTime.UtcNow;
            }
            else
            {
                var newInvite = new EventInviteRequest
                {
                    SenderId = currentUser.Id,
                    ReceiverId = userToAcceptId,
                    Event = eventFound,
                    DateSent = DateTime.UtcNow,
                    DateAccepted = DateTime.UtcNow
                };
                context.EventInviteRequests.Add(newInvite);
            }

            eventFound.Participants.Add(userToAccept);
            await context.SaveChangesAsync();

            return Result.Success;
        }
        
            /// <summary>
    /// Reject a user from an event
    /// </summary>
    /// <param name="eventId">Event ID</param>
    /// <param name="userToRejectId">ID of the user to be rejected</param>
    /// <param name="currentUser">Current user (event creator)</param>
    /// <returns></returns>
    [ErrorCode(null, ErrorCodes.NotFound, "Event not found")]
    [ErrorCode(null, ErrorCodes.AccessDenied, "Only the event creator can reject users")]
    [ErrorCode(null, ErrorCodes.UserNotInterestedInEvent, "User is not interested in the event")]
    [ErrorCode(null, ErrorCodes.UserAlreadyRejected, "User is already rejected from the event")]
    public async Task<Result> RejectUserFromEventAsync(int eventId, string userToRejectId, User currentUser)
    {
        var eventFound = await context.Events
            .Include(e => e.Interested)
            .Include(e => e.Creator)
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == eventId && !e.IsDeleted);

        if (eventFound == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Event not found",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        if (eventFound.CreatorId != currentUser.Id)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "Only the event creator can reject users",
                ErrorCode = ErrorCodes.AccessDenied
            };
        }

        var userToReject = await context.Users.FindAsync(userToRejectId);
        if (userToReject == null || eventFound.Interested.All(u => u.Id != userToRejectId))
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "User is not interested in the event",
                ErrorCode = ErrorCodes.UserNotInterestedInEvent
            };
        }

        var existingInvite = await context.EventInviteRequests
            .FirstOrDefaultAsync(ei => ei.Event.Id == eventId && ei.ReceiverId == userToRejectId && !ei.IsDeleted);

        if (existingInvite == null)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "No active invite found for this user",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        if (existingInvite.DateDeleted.HasValue)
        {
            return new ValidationFailure
            {
                PropertyName = null,
                ErrorMessage = "User is already rejected from the event",
                ErrorCode = ErrorCodes.UserAlreadyRejected
            };
        }

        existingInvite.DateDeleted = DateTime.UtcNow;
        eventFound.Interested.Remove(userToReject);
        eventFound.Participants.Remove(userToReject);

        await context.SaveChangesAsync();

        return Result.Success;
    }
            
        /// <summary>
        /// Create an event invite
        /// </summary>
        /// <param name="senderId">Sender ID</param>
        /// <param name="receiverId">Receiver ID</param>
        /// <param name="eventId">Event ID</param>
        /// <returns></returns>
        [ErrorCode(nameof(receiverId), ErrorCodes.NotFound)]
        [ErrorCode(nameof(eventId), ErrorCodes.NotFound)]
        [ErrorCode(nameof(receiverId), ErrorCodes.Duplicate, "Event invite already exists")]
        public async Task<Result> SendEventInviteAsync(string senderId, string receiverId, int eventId)
        {
            var receiverExists = await context.Users.AnyAsync(u => u.Id == receiverId);
            if (!receiverExists)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(receiverId),
                    ErrorMessage = "Receiver user does not exist",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var eventExists = await context.Events.AnyAsync(e => e.Id == eventId);
            if (!eventExists)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(eventId),
                    ErrorMessage = "Event does not exist",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var existingInvite = await context.EventInviteRequests
                .FirstOrDefaultAsync(ei =>
                    ei.SenderId == senderId && ei.ReceiverId == receiverId && ei.Event.Id == eventId && !ei.IsDeleted
                );

            if (existingInvite != null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(receiverId),
                    ErrorMessage = "Event invite already exists",
                    ErrorCode = ErrorCodes.Duplicate
                };
            }

            var eventInvite = new EventInviteRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                DateSent = DateTime.UtcNow,
                Event = (await context.Events.FindAsync(eventId))!
            };

            context.EventInviteRequests.Add(eventInvite);
            await context.SaveChangesAsync();
            return Result.Success;
        }
        
        /// <summary>
        /// Remove user from event's interested list
        /// </summary>
        [ErrorCode(null, ErrorCodes.NotFound)]
        [ErrorCode(null, ErrorCodes.Duplicate, "User is not interested in the event")]
        public async Task<Result> DeleteUserFromEventAsync(int id, User user)
        {
            var eventFound = await context.Events
                .Include(e => e.Interested)
                .Where(e=>e.Id==id)
                .FirstOrDefaultAsync();
            if(eventFound==null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "Event not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            if (!eventFound.Interested.Remove(user))
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "User is not interested in the event",
                    ErrorCode = ErrorCodes.UserNotInterestedInEvent
                };
            }
            await context.SaveChangesAsync();

            return Result.Success;
        }


        /// <summary>
        /// Get future events the user is interested in
        /// </summary>
        /// <param name="user">User whos intered event we go over</param>
        /// <param name="page">Page number to return.</param>
        /// <param name="perPage">Items per page.</param>
        /// <returns>Paginated list of events in which user is interested</returns>
        public async Task<Result<Pagination<EventSummaryVM>>> GetEventsInterestedInAsync(User user, int page, int perPage)
        {
            var query = context.Users
                .Where(u => u.Id == user.Id)
                .SelectMany(u => u.InterestedIn)
                .Where(u => u.Time > DateTime.UtcNow)
                .OrderBy(u => u.Time)
                .Select(e => new EventSummaryVM
                {
                    EventId = e.Id,
                    Description = e.Description,
                    Time = e.Time,
                    MaxPeople = e.MaxPeople,
                    MustJoinUntil = e.MustJoinUntil,
                    CreatorId = e.CreatorId,
                    CreatorFullName = e.Creator.FullName,
                    RestaurantId = e.RestaurantId,
                    RestaurantName = e.Restaurant.Name,
                    NumberInterested = e.Interested.Count
                });

            return await query.PaginateAsync(page, perPage, []);
        }


        /// <summary>
        /// Updates the details of an existing event.
        /// </summary>
        /// <param name="eventId">The id of the event to update.</param>
        /// <param name="request">The new details for the event.</param>
        /// <param name="user">The user updating the event.</param>
        /// <returns>A Result object containing the updated event or validation failures.</returns>
        [ErrorCode(null, ErrorCodes.NotFound)]
        [ErrorCode(null, ErrorCodes.AccessDenied, "Only the user who created the event can modify it")]
        [ErrorCode(nameof(UpdateEventRequest.RestaurantId), ErrorCodes.RestaurantDoesNotExist, "Restaurant with ID not found")]
        [ValidatorErrorCodes<Event>]
        public async Task<Result<EventVM>> UpdateEventAsync(int eventId, UpdateEventRequest request, User user)
        {
           var eventToUpdate = await context.Events
            .Include(e => e.Creator)
            .Include(e => e.Restaurant)
            .Include(e => e.Interested)
            .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventToUpdate is null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.NotFound,
                    ErrorMessage = "Event not found"
                };
            }

            if(eventToUpdate.Creator!=user)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.AccessDenied,
                    ErrorMessage = "Only the user who created the event can modify it"
                };
            }

            var restaurant = await context.Restaurants.FindAsync(request.RestaurantId);
            if (restaurant is null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(request.RestaurantId),
                    ErrorCode = ErrorCodes.RestaurantDoesNotExist,
                    ErrorMessage = $"Restaurant with ID {request.RestaurantId} not found"
                };
            }

            eventToUpdate.Description = request.Description;
            eventToUpdate.Time = request.Time;
            eventToUpdate.MaxPeople = request.MaxPeople;
            eventToUpdate.MustJoinUntil = request.MustJoinUntil;
            eventToUpdate.RestaurantId = request.RestaurantId;
            eventToUpdate.Restaurant = restaurant;

            var result = await validationService.ValidateAsync(eventToUpdate, user.Id);
            if (!result.IsValid)
            {
                return result;
            }

            await context.SaveChangesAsync();

            return new EventVM
            {
                EventId = eventToUpdate.Id,
                CreatedAt = eventToUpdate.CreatedAt,
                Description = eventToUpdate.Description,
                Time = eventToUpdate.Time,
                MaxPeople = eventToUpdate.MaxPeople,
                MustJoinUntil = eventToUpdate.MustJoinUntil,
                CreatorId = eventToUpdate.CreatorId,
                CreatorFullName = eventToUpdate.Creator.FullName,
                RestaurantId = eventToUpdate.RestaurantId,
                RestaurantName = eventToUpdate.Restaurant.Name,
                VisitId = eventToUpdate.VisitId,
                Interested = eventToUpdate.Interested.Select(i => new UserSummaryVM
                {
                    FirstName = i.FirstName,
                    LastName = i.LastName,
                    UserId = i.Id,
                    Photo = uploadService.GetPathForFileName(i.PhotoFileName),
                }).ToList()
            };
        }

        /// <summary>
        /// Deletes an existing event.
        /// </summary>
        /// <param name="eventId">The id of the event to delete.</param>
        /// <param name="user">User to check permissions</param>
        /// <returns>A Result object indicating success or a validation failure.</returns>
        [ErrorCode(null, ErrorCodes.NotFound)]
        [ErrorCode(null, ErrorCodes.AccessDenied, "Only the user who created the event can delete it")]
        public async Task<Result> DeleteEventAsync(int eventId, User user)
        {
            var eventToDelete = await context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventToDelete is null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.NotFound,
                    ErrorMessage = "Event not found"
                };
            }

            if(eventToDelete.CreatorId != user.Id)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.AccessDenied,
                    ErrorMessage = "Only the user who created the event can delete it"
                };
            }

            context.Events.Remove(eventToDelete);
            await context.SaveChangesAsync();

            return Result.Success;
        }
    }
}
