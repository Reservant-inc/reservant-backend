using System.IdentityModel.Tokens.Jwt;
using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.Auth;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Registration and signing in and out.
/// </summary>
[ApiController, Route("/auth")]
public class AuthController(
    UserService userService,
    UserManager<User> userManager,
    AuthService authService)
    : StrictController
{
    private readonly JwtSecurityTokenHandler _handler = new();

    /// <summary>
    /// Endpoint for restaurant owners to register their employees.
    /// </summary>
    /// <remarks>
    /// Actual user login is set to '{current user's login}+{login from request}'.
    /// </remarks>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("register-restaurant-employee"), Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200),
     ProducesResponseType(400),
     ProducesResponseType(401)]
    [MethodErrorCodes<UserService>(nameof(UserService.RegisterRestaurantEmployeeAsync))]
    public async Task<ActionResult<UserVM>> RegisterRestaurantEmployee(RegisterRestaurantEmployeeRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await userService.RegisterRestaurantEmployeeAsync(request, user);
        if (result.IsError) {
            return result.ToValidationProblem();
        }

        var employee = result.Value;
        return Ok(new UserVM
        {
            UserId = employee.Id,
            Login = employee.UserName!,
            Roles = await userManager.GetRolesAsync(employee),
            PhoneNumber = employee.FullPhoneNumber
        });
    }

    /// <summary>
    /// Register a CustomerSupportAgent.
    /// </summary>
    [HttpPost("register-customer-support-agent")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<UserService>(nameof(UserService.RegisterCustomerSupportAgentAsync))]
    public async Task<ActionResult<UserVM>> RegisterCustomerSupportAgent(RegisterCustomerSupportAgentRequest request)
    {
        var result = await userService.RegisterCustomerSupportAgentAsync(request);
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        var user = result.Value;
        return Ok(new UserVM
        {
            UserId = user.Id,
            Login = user.UserName!,
            Roles = await userManager.GetRolesAsync(user),
            PhoneNumber = user.FullPhoneNumber
        });
    }

    /// <summary>
    /// Login authorization
    /// </summary>
    /// <remarks>
    /// Returns a JWT bearer token intended to be sent in the Authorization header.
    ///
    /// If "firebaseDeviceToken" is set, registers the token to send push notifications to
    /// a device. For now only one device token is supported at a time. If a second one is
    /// registered, the first one stops receiving notifications.
    /// </remarks>
    /// <param name="request"> Login request DTO</param>
    /// <request code="400"> Validation errors </request>
    /// <request code="401"> Incorrect login or password </request>
    [HttpPost("login")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<ActionResult<UserInfo>> LoginUser(LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.Login.Trim());
        if (user is null || user.IsArchived)
        {
            return Problem(
                title: "Incorrect login or password",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Problem(
                title: "Incorrect login or password",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        if (!string.IsNullOrEmpty(request.FirebaseDeviceToken))
        {
            user.FirebaseDeviceToken = request.FirebaseDeviceToken;
            await userManager.UpdateAsync(user);
        }

        var token = authService.GenerateSecurityToken(user);
        var jwt = _handler.WriteToken(token);
        var roles = await userManager.GetRolesAsync(user);
        return Ok(new UserInfo
        {
            UserId = user.Id,
            Token = jwt,
            Login = request.Login,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Language = user.Language.ToString(),
            Roles = roles.ToList()
        });
    }

    /// <summary>
    /// Register a Firebase device token to send push notifications to the mobile device
    /// </summary>
    /// <remarks>
    /// For now only one device token is supported at a time. If a second one is
    /// registered, the first one stops receiving notifications.
    /// </remarks>
    [HttpPost("register-firebase-token")]
    [Authorize]
    [ProducesResponseType(200), ProducesResponseType(401)]
    public async Task<ActionResult> RegisterFirebaseToken(FirebaseTokenRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        user.FirebaseDeviceToken = request.DeviceToken;
        await userManager.UpdateAsync(user);
        return Ok();
    }

    /// <summary>
    /// Unregister a Firebase device token, so that the device stops receiving push notifications
    /// </summary>
    [HttpPost("unregister-firebase-token")]
    [Authorize]
    [ProducesResponseType(200), ProducesResponseType(401)]
    public async Task<ActionResult> UnregisterFirebaseToken(FirebaseTokenRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        if (user.FirebaseDeviceToken == request.DeviceToken)
        {
            user.FirebaseDeviceToken = null;
            await userManager.UpdateAsync(user);
        }

        return Ok();
    }

    /// <summary>
    /// Register a customer.
    /// </summary>
    [HttpPost("register-customer")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<UserService>(nameof(UserService.RegisterCustomerAsync))]
    public async Task<ActionResult<UserVM>> RegisterCustomer(RegisterCustomerRequest request)
    {
        var result = await userService.RegisterCustomerAsync(request);
        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        var user = result.Value;
        return Ok(new UserVM
        {
            UserId = user.Id,
            Login = user.UserName!,
            Roles = await userManager.GetRolesAsync(user),
            PhoneNumber = user.FullPhoneNumber
        });
    }

    /// <summary>
    /// Check if login is available
    /// </summary>
    [HttpGet("is-unique-login")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<bool>> IsUniqueLogin(String login)
    {
        var result = await userService.IsUniqueLoginAsync(login);

        return Ok(result);
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    /// <returns></returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(200)]
    [Authorize]
    public async Task<ActionResult<UserInfo>> RefreshTokenAsync() {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var token = authService.GenerateSecurityToken(user);
        var jwt = _handler.WriteToken(token);
        var roles = await userManager.GetRolesAsync(user);
        return Ok(new UserInfo
        {
            UserId = user.Id,
            Token = jwt,
            Login = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Language = user.Language.ToString(),
            Roles = roles.ToList()
        });
    }

    /// <summary>
    /// Change the current user's password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public async Task<ActionResult> ChangePassword(ChangePasswordRequest dto)
    {
        return OkOrErrors(await authService.ChangePassword(dto, User.GetUserId()!.Value));
    }
}
