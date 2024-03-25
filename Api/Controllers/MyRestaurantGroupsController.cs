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
/// Controller for managing RestaurantGroups for the current restaurant owner
/// </summary>
/// <param name="userManager"></param>
/// <param name="service"></param>
[ApiController, Route("/my-restaurant-groups")]
[Authorize(Roles = Roles.RestaurantOwner)]
public class MyRestaurantGroupsController(UserManager<User> userManager, RestaurantGroupService service) : Controller
{

    /// <summary>
    /// Post request to create a new RestaurantGroup. Only available for restaurant owners
    /// </summary>
    /// <param name="req">Request dto</param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(201), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<ActionResult> CreateRestaurantGroup(CreateRestaurantGroupRequest req)
    {

        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.CreateRestaurantGroup(req, user);

        if (result.IsError)
        {
            ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
            return BadRequest(ModelState);
        }

        return Created(result.Value.Id.ToString(), "");

    }

    /// <summary>
    /// Get groups of restaurants asociated with logged in restaurant owner
    /// </summary>
    /// <returns>RestaurantGroupSummaryVM</returns>
    [HttpGet]
    [ProducesResponseType(200),ProducesResponseType(404)]
    public async Task<ActionResult<List<RestaurantGroupSummaryVM>>> GetMyRestaurantGroupSummary()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var result = await service.GetUsersRestaurantGroupSummary(user);
        return Ok(result.Value);
    }
}
