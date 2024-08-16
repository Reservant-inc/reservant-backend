using ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.Event;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers
{
    /// <summary>
    /// Controller for managing events
    /// </summary>
    [ApiController, Route("/events")]
    public class EventsController(EventService service, UserManager<User> userManager) : StrictController
    {
        /// <summary>
        /// Create new event
        /// </summary>
        [HttpPost]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [Authorize(Roles = Roles.Customer)]
        [MethodErrorCodes<EventService>(nameof(EventService.CreateEventAsync))]
        public async Task<ActionResult<EventVM>> CreateEvent([FromBody] CreateEventRequest request)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            return OkOrErrors(await service.CreateEventAsync(request, user));
        }

        /// <summary>
        /// Get info about a specific event
        /// </summary>
        [HttpGet("{eventId:int}")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult<EventVM>> GetEvent(int eventId) {
            return OkOrErrors(await service.GetEventAsync(eventId));
        }




        /// <summary>
        /// Add logged-in user to event's interested list
        /// </summary>
        /// <param name="eventId"> Id of Event</param>
        /// <returns></returns>
        [HttpPost("{eventId:int}/interested")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [Authorize(Roles = Roles.Customer)]
        [MethodErrorCodes<EventService>(nameof(EventService.AddUserToEventAsync))]
        public async Task<ActionResult> AddUserToEvent(int eventId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            return OkOrErrors(await service.AddUserToEventAsync(eventId, user));
        }

        /// <summary>
        /// Remove logged-in user to event's interested list
        /// </summary>
        /// <param name="eventId"> Id of Event</param>
        /// <returns></returns>
        [HttpDelete("{eventId:int}/interested")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [Authorize(Roles = Roles.Customer)]
        [MethodErrorCodes<EventService>(nameof(EventService.DeleteUserFromEventAsync))]
        public async Task<ActionResult> DeleteUserFromEvent(int eventId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            return OkOrErrors(await service.DeleteUserFromEventAsync(eventId, user));
        }






        /// <summary>
        /// Update an existing event
        /// </summary>
        /// <param name="eventId"> Id of Event</param>
        /// <param name="request">New event info</param>
        /// <returns></returns>
        [HttpPut("{eventId:int}")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [Authorize(Roles = Roles.Customer)]
        [MethodErrorCodes<EventService>(nameof(EventService.UpdateEventAsync))]
        public async Task<ActionResult<EventVM>> UpdateEvent(int eventId, UpdateEventRequest request)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await service.UpdateEventAsync(eventId, request, user);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Delete an existing event
        /// </summary>
        /// <param name="eventId"> Id of Event</param>
        /// <returns></returns>
        [HttpDelete("{eventId:int}")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [Authorize(Roles = Roles.Customer)]
        [MethodErrorCodes<EventService>(nameof(EventService.DeleteEventAsync))]
        public async Task<ActionResult> DeleteEvent(int eventId)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            return OkOrErrors(await service.DeleteEventAsync(eventId, user));
        }
    }
}
