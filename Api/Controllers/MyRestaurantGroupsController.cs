using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.RestaurantGroups;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;
using Reservant.ErrorCodeDocs.Attributes;
using Reservant.Api.Dtos.Restaurants;
using Reservant.Api.Services.RestaurantServices;

namespace Reservant.Api.Controllers;


/// <summary>
/// Controller for managing RestaurantGroups for the current restaurant owner
/// </summary>
/// <param name="userManager"></param>
/// <param name="service"></param>
[ApiController, Route("/my-restaurant-groups")]
public class MyRestaurantGroupsController(UserManager<User> userManager, RestaurantGroupService service)
    : StrictController
{

    /// <summary>
    /// Post request to create a new RestaurantGroup
    /// </summary>
    /// <param name="req">Request dto</param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(401)]
    [MethodErrorCodes<RestaurantGroupService>(nameof(RestaurantGroupService.CreateRestaurantGroup))]
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
    [Authorize(Roles = $"{Roles.RestaurantOwner},{Roles.Customer}")]
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
    [Authorize(Roles = $"{Roles.RestaurantOwner},{Roles.Customer}")]
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
            if (ex.Message.Contains("not found", StringComparison.InvariantCultureIgnoreCase))
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
    [Authorize(Roles = Roles.RestaurantOwner)]
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

    /// <summary>
    /// Retrives restaurant statistics by restaurant group id and given time period
    /// </summary>
    [HttpGet("{restaurantGroupId:int}/statistics")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<StatisticsService>(nameof(StatisticsService.GetStatsByRestaurantGroupIdAsync))]
    public async Task<ActionResult<RestaurantStatsVM>> GetStatsByRestaurantGroupId(
        int restaurantGroupId,
        [FromQuery] RestaurantStatsRequest request,
        [FromServices] StatisticsService statisticsService)
    {
        var userId = User.GetUserId();
        var result = await statisticsService.GetStatsByRestaurantGroupIdAsync(restaurantGroupId, userId!.Value, request);
        return OkOrErrors(result);
    }

}
