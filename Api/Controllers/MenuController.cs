using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Menu;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Menu item controller.
/// </summary>
[ApiController, Route("/menus")]
public class MenuController(RestaurantMenuService service, UserManager<User> userManager) : Controller
{

    /// <summary>
    /// Gets a single menu with details for a given menu ID and restaurant ID.
    /// </summary>
    /// <returns></returns>
    [HttpGet("/menus/{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<MenuVM>> GetSingleMenuById(int id)
    {
        var result = await service.GetSingleMenuAsync(id);

        if (!result.IsError) return Ok(result.Value);

        return result.ToValidationProblem();
    }

    /// <summary>
    /// Posts menu to the restaurant
    /// </summary>
    /// <param name="req">Request for Menu to be created.</param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateMenu(CreateMenuRequest req)
    {
        var user = await userManager.GetUserAsync(User);
        var result = await service.PostMenuToRestaurant(req, user);

        if (!result.IsError) return Ok(result.Value);

        return result.ToValidationProblem();
    }

    /// <summary>
    /// Adds a given menuItem ids to specific menu
    /// </summary>
    /// <param name="id">Id of menu</param>
    /// <param name="request">Request containing MenuItemIds</param>
    /// <returns>The created list of menuItems</returns>
    [HttpPost("/menus/{id:int}/items")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<MenuVM>> AddToMenu(int id, AddItemsRequest request)
    {
        var user = await userManager.GetUserAsync(User);

        var result = await service.AddItemsToMenuAsync(id, request, user);

        if (!result.IsError) return Ok(result.Value);

        return result.ToValidationProblem();
    }

    /// <summary>
    /// Update menu
    /// </summary>
    /// <param name="request">New data</param>
    /// <param name="id">ID of the menu</param>
    /// <returns></returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<MenuVM>> UpdateMenu(UpdateMenuRequest request, int id)
    {
        var user = await userManager.GetUserAsync(User);

        var result = await service.UpdateMenuAsync(request, id, user);

        if (!result.IsError) return Ok(result.Value);

        return result.ToValidationProblem();
    }

    /// <summary>
    /// Delete a menu
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(204), ProducesResponseType(400)]
    public async Task<ActionResult> DeleteMenu(int id)
    {
        var user = await userManager.GetUserAsync(User);

        var res = await service.DeleteMenuAsync(id, user);

        if (res.IsError)
        {
            ValidationUtils.AddErrorsToModel(res.Errors, ModelState);
            return ValidationProblem();
        }

        return NoContent();
    }
}
