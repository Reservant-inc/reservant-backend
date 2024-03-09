using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;

namespace Reservant.Api.Controllers;

[ApiController, Route("/test")]
public class TestController(UserManager<User> userManager): Controller
{
    [HttpGet("/restaurant-owner-only")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    public async Task<ActionResult<User>> GetRestaurantOwner()
    {
        return Ok(await userManager.GetUserAsync(User));
    }

    [HttpGet("/restaurant-employee-only")]
    [Authorize(Roles = Roles.RestaurantEmployee)]
    public async Task<ActionResult<User>> GetRestaurantEmployee()
    {
        return Ok(await userManager.GetUserAsync(User));
    }

    [HttpGet("/restaurant-backdoors-employee-only")]
    [Authorize(Roles = Roles.RestaurantBackdoorsEmployee)]
    public async Task<ActionResult<User>> GetRestaurantBackdoorsEmployee()
    {
        return Ok(await userManager.GetUserAsync(User));
    }

    [HttpGet("/restaurant-hall-employee-only")]
    [Authorize(Roles = Roles.RestaurantHallEmployee)]
    public async Task<ActionResult<User>> GetRestaurantHallEmployee()
    {
        return Ok(await userManager.GetUserAsync(User));
    }

    [HttpGet("/customer-support-agent-only")]
    [Authorize(Roles = Roles.CustomerSupportAgent)]
    public async Task<ActionResult<User>> GetCustomerSupportAgent()
    {
        return Ok(await userManager.GetUserAsync(User));
    }

    [HttpGet("/customer-support-manager-only")]
    [Authorize(Roles = Roles.CustomerSupportManager)]
    public async Task<ActionResult<User>> GetCustomerSupportManager()
    {
        return Ok(await userManager.GetUserAsync(User));
    }

    [HttpGet("/customer-only")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<User>> GetCustomer()
    {
        return Ok(await userManager.GetUserAsync(User));
    }

    [HttpGet("/current-user")]
    [Authorize]
    public async Task<ActionResult<User>> GetCurrentUser()
    {
        return Ok(await userManager.GetUserAsync(User));
    }
}
