using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Services;

namespace Reservant.Api.Controllers;

/// <summary>
/// Managing Restaurant Groups
/// </summary>
[ApiController, Route("/my-restaurant-groups")]
public class MyRestaurantGroupsController(RestaurantGroupService groupService, UserManager<User> userManager) : Controller
{

    /// <summary>
    /// Updates restaurant group information - RestaurantOwner only
    /// </summary>
    [HttpPut("{id:int}"), Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<RestaurantGroupVM>> UpdateRestaurantGroupInfo(int id, UpdateRestaurantGroupRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        var result = await groupService.UpdateRestaurantGroupAsync(id, request, user.Id);

        if (!result.IsError)
        {
            return Ok(result.Value);
        }

        if (result.Errors != null && result.Errors.Any())
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.ErrorMessage) });
        }
        
        return BadRequest("An unexpected error occurred.");
    }

    
}