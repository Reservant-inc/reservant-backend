using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Registration and signing in and out.
/// </summary>
[ApiController, Route("/auth")]
public class AuthController(UserService userService) : Controller
{
    /// <summary>
    /// Register a restaurant owner.
    /// </summary>
    [HttpPost("register-restaurant-owner")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult> RegisterRestaurantOwner(RegisterRestaurantOwnerRequest request)
    {
        var result = await userService.RegisterRestaurantOwnerAsync(request);
        if (result.IsError)
        {
            ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
            return BadRequest(ModelState);
        }

        return Ok();
    }
}
