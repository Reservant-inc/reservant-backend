using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers
{
    [ApiController, Route("/users")]
    public class UsersController(UserService userService) : Controller
    {
        /// <summary>
        /// Sets Restaurant Owner role for specified user
        /// </summary>
        /// <param name="id">id of the user</param>
        /// <returns></returns>
        [ProducesResponseType(200), ProducesResponseType(404)]
        [HttpPost("/{id}/make-restaurant-owner"), Authorize(Roles = Roles.CustomerSupportAgent)]
        public async Task<ActionResult> MakeRestaurantOwner(string id) {
            var result = await userService.MakeRestaurantOwnerAsync(id);
            if (result.IsError)
            {
                ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
                return ValidationProblem();
            }
            User user = result.Value;
            if (user == null){ return NotFound(); }
            return Ok();
        }
    }
}
