using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Event;
using Reservant.Api.Models.Dtos.User;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

namespace Reservant.Api.Services
{
    /// <summary>
    /// Service for event management
    /// </summary>
    /// <param name="context"></param>
    /// <param name="userManager"></param>
    public class EventService(ApiDbContext context, UserManager<User> userManager)
    {
        /// <summary>
        /// Action for 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Result<EventVM>> CreateEventAsync(CreateEventRequest request, User user)
        {
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
        public async Task<Result<List<EventSummaryVM>>> GetEventsCreatedAsync(User user)
        {
            var events = await context.Events
                .Include(e => e.Interested)
                .Include(e => e.Creator)
                .Include(e => e.Restaurant)
                .Where(e => e.CreatorId == user.Id)
                .ToListAsync();

            if (events is null)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.NotFound,
                    ErrorMessage = ErrorCodes.NotFound
                };
            }
            if (events.Count == 0)
            {
                return new ValidationFailure
                {
                    PropertyName = null,
                    ErrorCode = ErrorCodes.NotFound,
                    ErrorMessage = ErrorCodes.NotFound
                };
            }

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
    }

}
