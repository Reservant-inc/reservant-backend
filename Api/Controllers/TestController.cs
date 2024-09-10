using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;

namespace Reservant.Api.Controllers;

/// <summary>
/// Test endpoints
/// </summary>
[ApiController, Route("/test")]
public class TestController(UserManager<User> userManager) : StrictController
{
    /// <summary>
    /// Available only to RestaurantOwner
    /// </summary>
    [HttpGet("restaurant-owner-only")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    public async Task<ActionResult<User?>> GetRestaurantOwner()
    {
        return Ok(await userManager.GetUserAsync(User));
    }

    /// <summary>
    /// Available only to RestaurantEmployee
    /// </summary>
    [HttpGet("restaurant-employee-only")]
    [Authorize(Roles = Roles.RestaurantEmployee)]
    public async Task<ActionResult<User?>> GetRestaurantEmployee()
    {
        return Ok(await userManager.GetUserAsync(User));
    }

    /// <summary>
    /// Available only to RestaurantBackdoorsEmployee
    /// </summary>
    [HttpGet("restaurant-backdoors-employee-only")]
    [Authorize(Roles = Roles.RestaurantEmployee)]
    public async Task<ActionResult<User?>> GetRestaurantBackdoorsEmployee()
    {
        return Ok(await userManager.GetUserAsync(User));
    }

    /// <summary>
    /// Available only to RestaurantHallEmployee
    /// </summary>
    [HttpGet("restaurant-hall-employee-only")]
    [Authorize(Roles = Roles.RestaurantEmployee)]
    public async Task<ActionResult<User?>> GetRestaurantHallEmployee()
    {
        return Ok(await userManager.GetUserAsync(User));
    }

    /// <summary>
    /// Available only to CustomerSupportAgent
    /// </summary>
    [HttpGet("customer-support-agent-only")]
    [Authorize(Roles = Roles.CustomerSupportAgent)]
    public async Task<ActionResult<User?>> GetCustomerSupportAgent()
    {
        return Ok(await userManager.GetUserAsync(User));
    }

    /// <summary>
    /// Available only to CustomerSupportManager
    /// </summary>
    [HttpGet("customer-support-manager-only")]
    [Authorize(Roles = Roles.CustomerSupportManager)]
    public async Task<ActionResult<User?>> GetCustomerSupportManager()
    {
        return Ok(await userManager.GetUserAsync(User));
    }

    /// <summary>
    /// Available only to Customer
    /// </summary>
    [HttpGet("customer-only")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<User?>> GetCustomer()
    {
        return Ok(await userManager.GetUserAsync(User));
    }

    /// <summary>
    /// Available only to logged in users
    /// </summary>
    [HttpGet("current-user")]
    [Authorize]
    public async Task<ActionResult<User?>> GetCurrentUser()
    {
        return Ok(await userManager.GetUserAsync(User));
    }
}
