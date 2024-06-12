using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Event;
using Reservant.Api.Services;
using Reservant.Api.Validation;
using Reservant.Api.Models.Dtos;
using FluentValidation.Results;
using Reservant.Api.Validators;
using Reservant.Api.Data;


namespace Reservant.Api.Controllers
{
    /// <summary>
    /// Controller for managing events
    /// </summary>
    [ApiController, Route("/events")]
    public class EventsController(EventService service, UserManager<User> userManager, ApiDbContext context) : StrictController
    {
        /// <summary>
        /// Create new event
        /// </summary>
        [HttpPost]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [Authorize(Roles = Roles.Customer)]
        public async Task<ActionResult<EventVM>> CreateEvent([FromBody] CreateEventRequest request)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await service.CreateEventAsync(request, user);
            if (result.IsError)
            {
                return result.ToValidationProblem();
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Get info about a specific event
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult<EventVM>> GetEvent(int id) {
            var result = await service.GetEventAsync(id);
            if (result.IsError)
            {
                return result.ToValidationProblem();
            }
            return Ok(result.Value);
        }




        /// <summary>
        /// Adds logged in user to event of given id
        /// <param name="id"> Id of Event</param>
        /// <returns></returns>
        /// </summary>
        [HttpPost("{id:int}/interested")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [Authorize(Roles = Roles.Customer)]
        public async Task<ActionResult> AddUserToEvent(int id)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await service.AddUserToEventAsync(id, user);
            if (result.IsError)
            {
                return result.ToValidationProblem();
            }

            return Ok();
        }

        /// <summary>
        /// Delets logged in user from event of given id
        /// <param name="id"> Id of Event</param>
        /// <returns></returns>
        /// </summary>
        [HttpDelete("{id:int}/interested")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        [Authorize(Roles = Roles.Customer)]
        public async Task<ActionResult> DeleteUserFromEvent(int id)
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await service.DeleteUserFromEventAsync(id, user);
            if (result.IsError)
            {
                return result.ToValidationProblem();
            }

            return Ok();
        }
    }
}
