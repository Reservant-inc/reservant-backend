using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.RestaurantGroup;
using Reservant.Api.Identity;
using Reservant.Api.Models;
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
public class MyRestaurantGroupsController(UserManager<User> userManager, RestaurantGroupService service)
    : StrictController
{

    /// <summary>
    /// Post request to create a new RestaurantGroup
    /// </summary>
    /// <param name="req">Request dto</param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(201), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<ActionResult<RestaurantGroupVM>> CreateRestaurantGroup(CreateRestaurantGroupRequest req)
    {

        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.CreateRestaurantGroup(req, user);
        return OkOrErrors(result);

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
    [HttpGet("{groupId:int}"), Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<RestaurantGroupVM>> GetRestaurantGroup(int groupId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await service.GetRestaurantGroupAsync(groupId, user.Id);

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
    [HttpPut("{groupId:int}"), Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<RestaurantGroupVM>> UpdateRestaurantGroupInfo(int groupId, UpdateRestaurantGroupRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await service.UpdateRestaurantGroupAsync(groupId, request, user.Id);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Deletes a restaurant group with all the restaurants in it
    /// </summary>
    /// <param name="groupId">id of the restaurant group that will be deleted</param>
    /// <returns></returns>
    [HttpDelete("{groupId:int}")]
    [ProducesResponseType(204), ProducesResponseType(404)]
    public async Task<ActionResult> SoftDeleteRestaurantGroup(int groupId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await service.SoftDeleteRestaurantGroupAsync(groupId, user);
        return OkOrErrors(result);
    }

}
