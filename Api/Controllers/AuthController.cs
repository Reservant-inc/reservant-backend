using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Registration and signing in and out.
/// </summary>
[ApiController, Route("/auth")]
public class AuthController(UserService userService, SignInManager<User> signInManager) : Controller
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

    /// <summary>
    /// Login authorization
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<ActionResult> LoginUser(LoginRequest request)
    {
        var email = request.Email;
        var password = request.Password;
        var result = await signInManager.PasswordSignInAsync(email, password, false, false);

        if (result.Succeeded)
        {
            // TODO: zwracac userinfo
            return Ok("Zalogowano");
        }
        else
        {
            return Unauthorized("Błędny login lub hasło");
        }
    }
    
}
