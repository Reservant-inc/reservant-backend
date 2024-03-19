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
    /// Controller for registering new employees in a restaurant
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("register-restaurant-employee")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult> RegisterRestaurantEmployee(RegisterRestaurantEmployeeRequest request)
    {
        var result = await userService.RegisterRestaurantEmployeeAsync(request);
        if (result.IsError) {
            ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
            return BadRequest(ModelState);
        }

        return Ok();
    }

    /// <summary>
    /// Register a CustomerSupportAgent.
    /// </summary>
    [HttpPost("register-customer-support-agent")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult> RegisterCustomerSupportAgent(RegisterCustomerSupportAgentRequest request)
    {
        var result = await userService.RegisterCustomerSupportAgentAsync(request);
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
    /// <remarks>
    /// Sets cookie named ".AspNetCore.Identity.Application".
    /// </remarks>
    /// <param name="request"> Login request DTO</param>
    /// <request code="400"> Validation errors </request>
    /// <request code="401"> Unauthorized </request>
    [HttpPost("login")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<ActionResult<UserInfo>> LoginUser(LoginRequest request)
    {
        var username = request.Username;
        var password = request.Password;
        var rememberPassword = request.RememberMe;

        var user = await userManager.FindByNameAsync(username);
        if (user == null) return Unauthorized("Incorrect username or password.");

        var result = await signInManager.PasswordSignInAsync(username, password, rememberPassword, false);

        var roles = await userManager.GetRolesAsync(user);

        return result.Succeeded switch
        {
            true => Ok(new UserInfo
            {
                Username = username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList()
            }),
            false => Unauthorized("Incorrect username or password.")
        };
    }

    /// <summary>
    /// Logging User out
    /// </summary>
    /// <remarks>
    /// Deletes cookie named ".AspNetCore.Identity.Application".
    /// </remarks>
    [HttpPost("logout"), Authorize]
    [ProducesResponseType(200), ProducesResponseType(401)]
    public async Task<ActionResult> LogoutUser()
    {
        await signInManager.SignOutAsync();
        return Ok("Logged out.");
    }

    /// <summary>
    /// Register a customer.
    /// </summary>
    [HttpPost("register-customer")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult> RegisterCustomer(RegisterCustomerRequest request)
    {
        var result = await userService.RegisterCustomerAsync(request);
        if (result.IsError)
        {
            ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
            return BadRequest(ModelState);
        }

        return Ok();
    }
}
