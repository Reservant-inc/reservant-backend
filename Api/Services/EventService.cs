using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Event;
using Reservant.Api.Models.Dtos.User;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services
{
    /// <summary>
    /// Service for event management
    /// </summary>
    /// <param name="context"></param>
    public class EventService(ApiDbContext context, ValidationService validationService)
    {
        /// <summary>
        /// Action for
        /// </summary>
        /// <param name="request"></param>
        /// <param name="user"></param>
        /// <returns></returns>
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
                Id = newEvent.Id,
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
                    UserId = i.Id
                }).ToList()
            };
        }

        /// <summary>
        /// Get information about an Event
        /// </summary>
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
                Id = checkedEvent.Id,
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
                    UserId = i.Id
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
                RestaurantName = e.Restaurant.Name,
                RestaurantId = e.Restaurant.Id,
                NumberInterested = e.Interested.Count,
                MustJoinUntil = e.MustJoinUntil,
                Id = e.Id,
                Description = e.Description,
                CreatorId = e.CreatorId,
            }).ToList();
        }

        public async Task<Result<bool>> AddUserToEventAsync(int id, User user)
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

            return true;
        }


        public async Task<Result<bool>> DeleteUserFromEventAsync(int id, User user)
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

            return true;
        }


        /// <summary>
        /// Get future events in a restaurant with pagination.
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
                .Select(e => new EventSummaryVM
                {
                    Id = e.Id,
                    Description = e.Description,
                    Time = e.Time,
                    MustJoinUntil = e.MustJoinUntil,
                    CreatorId = e.CreatorId,
                    CreatorFullName = e.Creator.FullName,
                    RestaurantId = e.RestaurantId,
                    RestaurantName = e.Restaurant.Name,
                    NumberInterested = e.Interested.Count
                });

            return await query.PaginateAsync(page, perPage);
        }
    }

}
