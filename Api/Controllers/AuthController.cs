using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Registration and signing in and out.
/// </summary>
[ApiController, Route("/auth")]
public class AuthController(UserService userService, SignInManager<User> signInManager, UserManager<User> userManager) : Controller
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
    /// Login authorization - Sets Cookie
    /// </summary>
    /// <param name="request"> Login request DTO</param>
    /// <request code="400"> Validation errors </request>
    /// <request code="401"> Unauthorized </request>
    [HttpPost("login")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<ActionResult> LoginUser(LoginRequest request)
    {
        var email = request.Email;
        var password = request.Password;
        var rememberPassword = request.RememberMe;
        
        var user = await userManager.FindByEmailAsync(email);
        if (user == null) return Unauthorized("Incorrect login or password.");
        
        var result = await signInManager.PasswordSignInAsync(email, password, rememberPassword, false);

        var roles = await userManager.GetRolesAsync(user);
        
        return result.Succeeded switch
        {
            true => Ok(new UserInfo { Username = email, Roles = roles.ToList() }),
            false => Unauthorized("Incorrect login or password.")
        };
    }

    /// <summary>
    /// Logging User out - deletes Cookie
    /// </summary>
    [HttpPost("logout"), Authorize]
    [ProducesResponseType(200), ProducesResponseType(401)]
    public async Task<ActionResult> LogoutUser()
    {
        await signInManager.SignOutAsync();
        return Ok("Logged out.");
    }
    
    
}
