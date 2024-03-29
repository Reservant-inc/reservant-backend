using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.Auth;
using Reservant.Api.Options;
using Reservant.Api.Services;
using Reservant.Api.Validation;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Reservant.Api.Controllers;

/// <summary>
/// Registration and signing in and out.
/// </summary>
[ApiController, Route("/auth")]
public class AuthController(
    UserService userService,
    UserManager<User> userManager,
    IOptions<JwtOptions> jwtOptions)
    : Controller
{
    private readonly JwtSecurityTokenHandler _handler = new();

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
            return ValidationProblem();
        }

        return Ok();
    }

    /// <summary>
    /// Endpoint for restaurant owners to register their employees.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("register-restaurant-employee"), Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200),
     ProducesResponseType(400),
     ProducesResponseType(401)]
    public async Task<ActionResult> RegisterRestaurantEmployee(RegisterRestaurantEmployeeRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        var result = await userService.RegisterRestaurantEmployeeAsync(request, user);
        if (result.IsError) {
            ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
            return ValidationProblem();
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
            return ValidationProblem();
        }

        return Ok();
    }

    /// <summary>
    /// Login authorization
    /// </summary>
    /// <remarks>
    /// Returns a JWT bearer token intended to be sent in the Authorization header.
    /// </remarks>
    /// <param name="request"> Login request DTO</param>
    /// <request code="400"> Validation errors </request>
    /// <request code="401"> Incorrect login or password </request>
    [HttpPost("login")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<ActionResult<UserInfo>> LoginUser(LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.Login.Trim());
        if (user is null)
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

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.Name, user.UserName!)
        };

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(TimeSpan.FromHours(jwtOptions.Value.LifetimeHours)),
            Issuer = jwtOptions.Value.Issuer,
            Audience = jwtOptions.Value.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(jwtOptions.Value.GetKeyBytes()),
                SecurityAlgorithms.HmacSha256)
        };

        var token = _handler.CreateToken(tokenDescriptor);
        var jwt = _handler.WriteToken(token);
        return Ok(new UserInfo
        {
            Token = jwt,
            Login = request.Login,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList()
        });
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
            return ValidationProblem();
        }

        return Ok();
    }
}
