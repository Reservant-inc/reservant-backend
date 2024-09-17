using FluentValidation.Results;
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
                RestaurantName = checkedEvent.Restaurant?.Name,
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
                RestaurantName = e.Restaurant?.Name,
                RestaurantId = e.RestaurantId,
                NumberInterested = e.Interested.Count,
                MustJoinUntil = e.MustJoinUntil,
                EventId = e.Id,
                Description = e.Description,
                CreatorId = e.CreatorId,
            }).ToList();
        }

        public async Task<Result> RequestParticipationAsync(int eventId, User user)
        {
            var eventFound = await context.Events.FindAsync(eventId);
            if (eventFound == null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "Event not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var existingRequest = await context.EventParticipationRequests
                .FirstOrDefaultAsync(pr => pr.EventId == eventId && pr.UserId == user.Id);

            if (existingRequest != null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "Request already exists",
                    ErrorCode = ErrorCodes.Duplicate
                };
            }

            var participationRequest = new ParticipationRequest
            {
                EventId = eventId,
                UserId = user.Id,
                RequestDate = DateTime.UtcNow
            };

            context.EventParticipationRequests.Add(participationRequest);
            await context.SaveChangesAsync();

            return Result.Success;
        }

        public async Task<Result> AcceptParticipationRequestAsync(int eventId, string userId, User currentUser)
        {
            var eventFound = await context.Events
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventFound == null || eventFound.CreatorId != currentUser.Id)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "Event not found or access denied",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var request = await context.EventParticipationRequests
                .Include(participationRequest => participationRequest.User)
                .FirstOrDefaultAsync(pr => pr.EventId == eventId && pr.UserId == userId);

            if (request is not { Accepted: null })
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "User already accepted",
                    ErrorCode = ErrorCodes.UserAlreadyAccepted
                };
            }

            if (eventFound.Participants.Count >= eventFound.MaxPeople)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "Event is full",
                    ErrorCode = ErrorCodes.EventIsFull
                };
            }

            if (DateTime.Compare(eventFound.MustJoinUntil, DateTime.Now) < 0)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "Join deadline passed",
                    ErrorCode = ErrorCodes.JoinDeadlinePassed
                };
            }

            request.Accepted = DateTime.Now;
            eventFound.Participants.Add(request.User);
            await context.SaveChangesAsync();

            return Result.Success;
        }

        public async Task<Result> RejectParticipationRequestAsync(int eventId, string userId, User currentUser)
        {
            var eventFound = await context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventFound == null || eventFound.CreatorId != currentUser.Id)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "Event not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var request = await context.EventParticipationRequests
                .FirstOrDefaultAsync(pr => pr.EventId == eventId && pr.UserId == userId);

            if (request is not { Rejected: null })
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "User already rejected",
                    ErrorCode = ErrorCodes.UserAlreadyRejected
                };
            }

            request.Rejected = DateTime.Now;
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
                    RestaurantName = e.Restaurant == null ? null : e.Restaurant.Name,
                    NumberInterested = e.Interested.Count,
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
