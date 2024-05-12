using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.RestaurantGroup;
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
            return result.ToValidationProblem();
        }

        return Created(result.Value.Id.ToString(), "");

    }

    /// <summary>
    /// Get groups of restaurants asociated with logged in restaurant owner
    /// </summary>
    /// <returns>RestaurantGroupSummaryVM</returns>
    [HttpGet]
    [ProducesResponseType(200), ProducesResponseType(401)]
    public async Task<ActionResult<List<RestaurantGroupSummaryVM>>> GetMyRestaurantGroupSummary()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.GetUsersRestaurantGroupSummary(user);
        return Ok(result.Value);
    }

    /// <summary>
    /// Retrieves information about a specific restaurant group
    /// </summary>
    [HttpGet("{id:int}"), Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<RestaurantGroupVM>> GetRestaurantGroup(int id)
    {
        var user = await userManager.GetUserAsync(User);
        var result = await service.GetRestaurantGroupAsync(id, user.Id);

        try
        {
            return Ok(result.OrThrow());
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound();
            }
            else
            {
                return Forbid();
            }
        }

    }

    /// <summary>
    /// Updates name of restaurant group
    /// </summary>
    [HttpPut("{id:int}"), Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<RestaurantGroupVM>> UpdateRestaurantGroupInfo(int id, UpdateRestaurantGroupRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        var result = await service.UpdateRestaurantGroupAsync(id, request, user.Id);

        if (!result.IsError)
        {
            return Ok(result.Value);
        }

        return result.ToValidationProblem();
    }

    /// <summary>
    /// Deletes a restaurant group
    /// </summary>
    /// <param name="id">id of the restaurant group that will be deleted</param>
    /// <returns></returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204), ProducesResponseType(404)]
    public async Task<ActionResult> SoftDeleteRestaurantGroup(int id)
    {
        var user = await userManager.GetUserAsync(User);
        var result = await service.SoftDeleteRestaurantGroupAsync(id, user);

        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return NoContent();
    }

}
