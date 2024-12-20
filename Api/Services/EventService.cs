using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Events;
using Reservant.Api.Dtos.Users;
using NetTopologySuite.Geometries;
using Reservant.Api.Dtos.Restaurants;
using Reservant.Api.Models.Enums;
using Reservant.Api.Dtos.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Reservant.Api.Services
{
    /// <summary>
    /// Service for event management
    /// </summary>
    public class EventService(
        ApiDbContext context,
        ValidationService validationService,
        NotificationService notificationService,
        GeometryFactory geometryFactory,
        IMapper mapper
        )
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
            var validationResult = await validationService.ValidateAsync(request, user.Id);
            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            // Tworzenie nowego Event
            var newEvent = new Event
            {
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
                Description = request.Description,
                Time = request.Time,
                MaxPeople = request.MaxPeople,
                MustJoinUntil = request.MustJoinUntil,
                CreatorId = user.Id,
                RestaurantId = request.RestaurantId,
                Creator = user,
                IsDeleted = false,
                PhotoFileName = request.Photo
            };

            if (request.RestaurantId != null)
            {
                var restaurant = await context.Restaurants
                    .OnlyActiveRestaurants()
                    .FirstOrDefaultAsync(restaurant => restaurant.RestaurantId == request.RestaurantId);

                if (restaurant is null)
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(request.RestaurantId),
                        ErrorCode = ErrorCodes.NotFound,
                        ErrorMessage = $"Restaurant with ID {request.RestaurantId} not found"
                    };
                }
                newEvent.Restaurant = restaurant;
            }

            // Tworzenie wątku dla eventu
            var thread = new MessageThread
            {
                Title = $"Discussion for Event: {newEvent.Name}",
                CreationDate = DateTime.UtcNow,
                CreatorId = user.Id,
                Participants = new List<User> { user },
                IsEditable = false // Wątek dla eventu nie może być edytowany
            };

            // Przypisanie wątku do eventu
            newEvent.Thread = thread;

            context.MessageThreads.Add(thread);
            await context.Events.AddAsync(newEvent);
            await context.SaveChangesAsync();

            return mapper.Map<EventVM>(newEvent);
        }


        /// <summary>
        /// Get information about an Event
        /// </summary>
        [ErrorCode(null, ErrorCodes.NotFound)]
        public async Task<Result<EventVM>> GetEventAsync(int id)
        {
            var checkedEvent = await context.Events
                .ProjectTo<EventVM>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(e => e.EventId == id);
            if (checkedEvent is null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.NotFound,
                    ErrorMessage = ErrorCodes.NotFound
                };
            }

            return checkedEvent;
        }

        /// <summary>
        /// Get events created by the given User
        /// </summary>
        public async Task<Result<List<EventSummaryVM>>> GetEventsCreatedAsync(User user)
        {
            return await context.Events
                .Where(e => e.CreatorId == user.Id)
                .ProjectTo<EventSummaryVM>(mapper.ConfigurationProvider)
                .ToListAsync();
        }

        /// <summary>
        /// Create a ParticipationRequest
        /// </summary>
        /// <param name="eventId">ID of the event</param>
        /// <param name="user">Request sender</param>
        ///         [ErrorCode(nameof(eventId), ErrorCodes.NotFound, "Event not found")]
        [ErrorCode(nameof(eventId), ErrorCodes.NotFound, "Event not found")]
        [ErrorCode(nameof(eventId), ErrorCodes.Duplicate, "Request already exists")]
        public async Task<Result> RequestParticipationAsync(int eventId, User user)
        {
            var eventFound = await context.Events.FindAsync(eventId);
            if (eventFound == null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(eventId),
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
                    PropertyName = nameof(eventId),
                    ErrorMessage = "Request already exists",
                    ErrorCode = ErrorCodes.Duplicate
                };
            }

            var participationRequest = new ParticipationRequest
            {
                EventId = eventId,
                UserId = user.Id,
                DateSent = DateTime.UtcNow
            };

            context.EventParticipationRequests.Add(participationRequest);
            await context.SaveChangesAsync();

            await notificationService.NotifyNewParticipationRequest(eventFound.CreatorId, user.Id, eventId);

            return Result.Success;
        }

        /// <summary>
        /// Get paginated list of users who are interested in an event but not accepted or rejected.
        /// </summary>
        /// <param name="eventId">ID of the event.</param>
        /// <param name="userId">ID of the current user for permission checking</param>
        /// <param name="page">Page number to return.</param>
        /// <param name="perPage">Items per page.</param>
        /// <returns>Paginated list of users with pending participation requests.</returns>
        [ErrorCode(nameof(eventId), ErrorCodes.NotFound, "Event not found")]
        [ErrorCode(nameof(eventId), ErrorCodes.AccessDenied, "User not creator of the event")]
        [MethodErrorCodes(typeof(Utils), nameof(Utils.PaginateAsync))]
        public async Task<Result<Pagination<UserSummaryVM>>> GetInterestedUsersAsync(int eventId, Guid userId, int page, int perPage)
        {
            var eventEntity = await context.Events.FindAsync(eventId);
            if (eventEntity == null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(eventId),
                    ErrorMessage = $"Event with ID {eventId} not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            if (eventEntity.CreatorId != userId)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(eventId),
                    ErrorMessage = $"User not creator of the event {eventId}",
                    ErrorCode = ErrorCodes.AccessDenied
                };
            }

            var query = context.EventParticipationRequests
                .Where(pr => pr.EventId == eventId && pr.DateAccepted == null && pr.DateDeleted == null)
                .OrderByDescending(pr => pr.DateSent)
                .ProjectTo<UserSummaryVM>(mapper.ConfigurationProvider);

            return await query.PaginateAsync(page, perPage, []);
        }

        /// <summary>
        /// Accept a ParticipationRequest as the Event creator
        /// </summary>
        /// <param name="eventId">ID of the event</param>
        /// <param name="userId">ID of the request sender</param>
        /// <param name="currentUser">Current user for permission checking</param>
        [ErrorCode(nameof(eventId), ErrorCodes.NotFound, "Event not found")]
        [ErrorCode(nameof(eventId), ErrorCodes.UserAlreadyAccepted, "User already accepted")]
        [ErrorCode(nameof(eventId), ErrorCodes.EventIsFull, "Event is full")]
        public async Task<Result> AcceptParticipationRequestAsync(int eventId, Guid userId, User currentUser)
        {
            // Pobranie Eventu z ParticipationRequests
            var eventFound = await context.Events
                .Include(e => e.ParticipationRequests)
                .Include(e => e.Thread) // Załaduj Thread, ale nie zakładaj, że ma uczestników
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventFound == null || eventFound.CreatorId != currentUser.Id)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(eventId),
                    ErrorMessage = "Event not found or you are not the creator",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            // Sprawdzenie, czy użytkownik istnieje w ParticipationRequests
            var request = eventFound.ParticipationRequests
                .FirstOrDefault(pr => pr.UserId == userId);

            if (request is not { DateAccepted: null })
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(userId),
                    ErrorMessage = "User already accepted",
                    ErrorCode = ErrorCodes.UserAlreadyAccepted
                };
            }

            // Sprawdzenie limitu uczestników
            var acceptedCount = eventFound.ParticipationRequests.Count(pr => pr.DateAccepted != null);
            if (acceptedCount >= eventFound.MaxPeople)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(eventId),
                    ErrorMessage = "Event is full",
                    ErrorCode = ErrorCodes.EventIsFull
                };
            }

            // Akceptacja użytkownika
            request.DateAccepted = DateTime.UtcNow;

            // Utworzenie Thread, jeśli go nie ma
            if (eventFound.Thread == null)
            {
                var newThread = new MessageThread
                {
                    Title = $"Discussion for Event: {eventFound.Name}",
                    CreationDate = DateTime.UtcNow,
                    CreatorId = currentUser.Id,
                    Participants = new List<User> { currentUser }
                };

                eventFound.Thread = newThread;
                context.MessageThreads.Add(newThread);
            }

            // Dodanie użytkownika do uczestników wątku
            if (eventFound.Thread.Participants == null)
            {
                eventFound.Thread.Participants = new List<User>();
            }

            var userToAdd = await context.Users.FindAsync(userId);
            if (userToAdd != null && !eventFound.Thread.Participants.Any(p => p.Id == userId))
            {
                eventFound.Thread.Participants.Add(userToAdd);
            }

            await context.SaveChangesAsync();
            await notificationService.NotifyParticipationRequestResponse(userId, eventId, true);

            return Result.Success;
        }

        /// <summary>
        /// Reject a ParticipationRequest as the event creator, or remove from participants if accepted.
        /// </summary>
        /// <param name="eventId">ID of the event</param>
        /// <param name="userId">ID of the request sender</param>
        /// <param name="currentUser">Current user for permission checking</param>
        [ErrorCode(nameof(eventId), ErrorCodes.NotFound, "Event not found")]
        [ErrorCode(nameof(userId), ErrorCodes.UserAlreadyRejected, "User already rejected")]
        public async Task<Result> RejectParticipationRequestAsync(int eventId, Guid userId, User currentUser)
        {
            var eventFound = await context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventFound == null || eventFound.CreatorId != currentUser.Id)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(eventId),
                    ErrorMessage = "Event not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }


            var request = await context.EventParticipationRequests
                .FirstOrDefaultAsync(pr => pr.EventId == eventId && pr.UserId == userId);

            if (request is not { DateDeleted: null })
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(userId),
                    ErrorMessage = "User already rejected",
                    ErrorCode = ErrorCodes.UserAlreadyRejected
                };
            }

            if(request is { DateAccepted: DateTime })
            {
                request.DateAccepted = null;
            }

            request.DateDeleted = DateTime.Now;
            await context.SaveChangesAsync();
            await notificationService.NotifyParticipationRequestResponse(userId, eventId, false);

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
                .Include(e => e.ParticipationRequests)
                .Where(e => e.EventId == id)
                .FirstOrDefaultAsync();
            if (eventFound == null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "Event not found",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var request = eventFound.ParticipationRequests
                .FirstOrDefault(r => r.UserId == user.Id);
            if (request is null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorMessage = "User is not interested in the event",
                    ErrorCode = ErrorCodes.UserNotInterestedInEvent
                };
            }

            request.DateDeleted = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return Result.Success;
        }


        /// <summary>
        /// Get a list of events that the user might have interest in.
        /// </summary>
        /// <param name="category">Value to filter the search results through</param>
        /// <param name="dateFrom">date used as a bottom boundry for the search</param>
        /// <param name="dateUntil">date used as a upper boundry for the search</param>
        /// <param name="order">value by which the search result should be ordered</param>
        /// <param name="user">User whos intered event we go over</param>
        /// <param name="page">Page number to return.</param>
        /// <param name="perPage">Items per page.</param>
        /// <returns>Paginated list of events in which user might be interested in.</returns>
        public async Task<Result<Pagination<EventSummaryVM>>> GetUserEventsAsync(EventParticipationCategory category, DateTime? dateFrom, DateTime? dateUntil, EventSorting order, User user, int page, int perPage)
        {
            IQueryable<Event> events = context.Events;

            switch (category)
            {
                case EventParticipationCategory.ParticipateIn:
                    events = context.Users
                        .Where(u => u.Id == user.Id)
                        .SelectMany(u => u.EventParticipations
                            .Where(e => e.DateDeleted == null && e.DateAccepted != null)
                            .Select(e => e.Event));
                    break;
                case EventParticipationCategory.InterestedIn:
                    events = context.Users
                        .Where(u => u.Id == user.Id)
                        .SelectMany(u => u.EventParticipations
                            .Where(e => e.DateAccepted == null)
                            .Select(e => e.Event));
                    break;
                case EventParticipationCategory.CreatedBy:
                    events = events.Where(e => e.CreatorId == user.Id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(category));
            }

            if (dateFrom != null)
            {
                events = events.Where(e => e.Time >= dateFrom);
            }

            if (dateUntil != null)
            {
                events = events.Where(e => e.Time <= dateUntil);
            }

            switch (order)
            {
                case EventSorting.DateDesc:
                    events = events.OrderBy(e => e.Time);
                    break;
                case EventSorting.DateAsc:
                    events = events.OrderByDescending(e => e.Time);
                    break;
                case EventSorting.DateCreatedAsc:
                    events = events.OrderBy(e => e.CreatedAt);
                    break;
                case EventSorting.DateCreatedDesc:
                    events = events.OrderByDescending(e => e.CreatedAt);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(order));
            }

            var result = events.ProjectTo<EventSummaryVM>(mapper.ConfigurationProvider);
            return await result.PaginateAsync(page, perPage, Enum.GetNames<EventSorting>(), 100, true);
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
        [ErrorCode(nameof(request.Time), ErrorCodes.DateMustBeInFuture, "If changed, must be in the future")]
        [ErrorCode(nameof(request.MustJoinUntil), ErrorCodes.DateMustBeInFuture, "If changed, must be in the future")]
        [ValidatorErrorCodes<Event>]
        public async Task<Result<EventVM>> UpdateEventAsync(int eventId, UpdateEventRequest request, User user)
        {
            var eventToUpdate = await context.Events
                .Include(e => e.Creator)
                .Include(e => e.Restaurant)
                .Include(e => e.ParticipationRequests)
                .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventToUpdate is null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.NotFound,
                    ErrorMessage = "Event not found"
                };
            }

            if (eventToUpdate.Creator != user)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.AccessDenied,
                    ErrorMessage = "Only the user who created the event can modify it"
                };
            }

            if (request.Time != eventToUpdate.Time && request.Time <= DateTime.UtcNow)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(request.Time),
                    ErrorCode = ErrorCodes.DateMustBeInFuture,
                    ErrorMessage = "Event time must be in the future"
                };
            }

            if (request.MustJoinUntil != eventToUpdate.MustJoinUntil && request.MustJoinUntil <= DateTime.UtcNow)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(request.MustJoinUntil),
                    ErrorCode = ErrorCodes.DateMustBeInFuture,
                    ErrorMessage = "MustJoinUntil must be in the future"
                };
            }

            eventToUpdate.Name = request.Name;
            eventToUpdate.Description = request.Description;
            eventToUpdate.Time = request.Time;
            eventToUpdate.MaxPeople = request.MaxPeople;
            eventToUpdate.MustJoinUntil = request.MustJoinUntil;
            eventToUpdate.PhotoFileName = request.Photo;

            if (request.RestaurantId is not null && request.RestaurantId != eventToUpdate.RestaurantId)
            {
                eventToUpdate.Restaurant = await context.Restaurants
                    .OnlyActiveRestaurants()
                    .SingleOrDefaultAsync(restaurant => restaurant.RestaurantId == request.RestaurantId);
                if (eventToUpdate.Restaurant is null)
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(request.RestaurantId),
                        ErrorCode = ErrorCodes.RestaurantDoesNotExist,
                        ErrorMessage = $"Restaurant with ID {request.RestaurantId} not found"
                    };
                }
            }
            else
            {
                eventToUpdate.Restaurant = null;
            }

            var result = await validationService.ValidateAsync(eventToUpdate, user.Id);
            if (!result.IsValid)
            {
                return result;
            }

            await context.SaveChangesAsync();

            return mapper.Map<EventVM>(eventToUpdate);
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
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventToDelete is null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.NotFound,
                    ErrorMessage = "Event not found"
                };
            }

            if (eventToDelete.CreatorId != user.Id)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.AccessDenied,
                    ErrorMessage = "Only the user who created the event can delete it"
                };
            }

            eventToDelete.IsDeleted = true;
            await context.SaveChangesAsync();

            return Result.Success;
        }

        /// <summary>
        /// Returns events with specified optional parameters
        /// </summary>
        /// <param name="request">parameters of the request</param>
        /// <param name="page">Page number to return.</param>
        /// <param name="perPage">Items per page.</param>
        /// <param name="user">The current user.</param>
        /// <returns>list of events that fulfill the requirements</returns>
        [ErrorCode(nameof(request.FriendsOnly), ErrorCodes.AccessDenied, "User must be logged in to see friends' events")]
        [ErrorCode(nameof(GetEventsRequest.OrigLon), ErrorCodes.InvalidSearchParameters)]
        [ErrorCode(nameof(GetEventsRequest.OrigLat), ErrorCodes.InvalidSearchParameters)]
        [MethodErrorCodes(typeof(Utils), nameof(Utils.PaginateAsync))]
        public async Task<Result<Pagination<NearEventVM>>> GetEventsAsync(GetEventsRequest request, int page, int perPage, User? user)
        {
            IQueryable<Event> events = context.Events;
            if (request.Name is not null)
            {
                events = events.Where(e => e.Name.Contains(request.Name.Trim()));
            }
            if (request.DateFrom is not null)
            {
                events = events.Where(e => e.Time > request.DateFrom);
            }
            if (request.DateUntil is not null)
            {
                events = events.Where(e => e.Time < request.DateUntil);
            }
            if (request.FriendsOnly)
            {
                if (user is null)
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(request.FriendsOnly),
                        ErrorCode = ErrorCodes.AccessDenied,
                        ErrorMessage = "User must be logged in to see friends' events",
                    };
                }

                events = events.Where(e => context.FriendRequests.Any(fr =>
                    ((fr.ReceiverId == user.Id && fr.SenderId == e.CreatorId) ||
                    (fr.SenderId == user.Id && fr.ReceiverId == e.CreatorId)) &&
                    fr.DateAccepted != null && fr.DateDeleted == null));
            }
            if (request.EventStatus is not null)
            {
                switch (request.EventStatus)
                {
                    case EventStatus.Future:
                        events = events.Where(e => e.MustJoinUntil > DateTime.UtcNow && e.MaxPeople > e.ParticipationRequests.Count);
                        break;
                    case EventStatus.Past:
                        events = events.Where(e => e.Time < DateTime.UtcNow);
                        break;
                    case EventStatus.NonJoinable:
                        events = events.Where(e => (e.MustJoinUntil < DateTime.UtcNow || e.MaxPeople == e.ParticipationRequests.Count) && e.Time > DateTime.UtcNow);
                        break;
                }
            }

            var origin = geometryFactory.CreatePoint();

            if (request.RestaurantName is not null && request.RestaurantId is null)
            {
                events = events.Where(e => e.Restaurant != null && e.Restaurant.Name.Contains(request.RestaurantName));
            }

            if (request.RestaurantId is not null)
            {
                events = events.Where(e => e.RestaurantId == request.RestaurantId);
            }
            else if (request.OrigLon is not null || request.OrigLat is not null)
            {
                if (request.OrigLon is null)
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(request.OrigLon),
                        ErrorCode = ErrorCodes.InvalidSearchParameters,
                        ErrorMessage = "Either both origLon and origLat must be specified or none",
                    };
                }

                if (request.OrigLat is null)
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(request.OrigLat),
                        ErrorCode = ErrorCodes.InvalidSearchParameters,
                        ErrorMessage = "Either both origLon and origLat must be specified or none",
                    };
                }

                if (!Utils.IsValidLatitude(request.OrigLat.Value))
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(request.OrigLat),
                        ErrorCode = ErrorCodes.InvalidSearchParameters,
                        ErrorMessage = "Latitude must in the range of [-180; 180]",
                    };
                }

                if (!Utils.IsValidLongitude(request.OrigLon.Value))
                {
                    return new ValidationFailure
                    {
                        PropertyName = nameof(request.OrigLon),
                        ErrorCode = ErrorCodes.InvalidSearchParameters,
                        ErrorMessage = "Longitude must in the range of [-90; 90]",
                    };
                }

                origin = geometryFactory.CreatePoint(new Coordinate(request.OrigLat.Value, request.OrigLon.Value));
                events = events.OrderBy(e => origin.Distance(e.Restaurant!.Location));
            }

            events = events.OrderByDescending(e => e.CreatedAt);

            return await events
                .ProjectTo<NearEventVM>(mapper.ConfigurationProvider, new { origin })
                .PaginateAsync(page, perPage, [], 100, false);
        }
    }
}
