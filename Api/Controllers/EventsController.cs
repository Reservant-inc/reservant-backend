using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Event;
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
        [HttpPost]
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
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventVM>> GetEvent(int id) {
            var result = await service.GetEventAsync(id);
            if (result.IsError)
            {
                return result.ToValidationProblem();
            }
            return Ok(result.Value);
        }
    }
}
