using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.MenuItem;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Controller for managing menu items for a logged in RestaurantOwner
/// </summary>
/// <param name="userManager"></param>
/// <param name="service"></param>

[ApiController, Route("/menu-items")]
public class MenuItemController(UserManager<User> userManager, MenuItemsService service) : StrictController
{

    /// <summary>
    /// Creates a menu item in the given restaurant
    /// </summary>
    /// <param name="menuItem">Item to be created</param>
    /// <returns>The created menuItem</returns>
    [HttpPost]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(201), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<ActionResult<MenuItemVM>> CreateMenuItems(CreateMenuItemRequest menuItem)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var res = await service.CreateMenuItemsAsync(user, menuItem);

        if (res.IsError)
        {
            return res.ToValidationProblem();
        }

        return Created(null, res.Value);
    }


    /// <summary>
    /// Gets menu item by given id
    /// </summary>
    /// <param name="menuItemId">Id of the menuItem</param>
    /// <returns>The found menu item</returns>
    [HttpGet]
    [Route("{menuItemId:int}")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<ActionResult<MenuItemVM>> GetMenuItemById(int menuItemId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var res = await service.GetMenuItemByIdAsync(user, menuItemId);

        if (res.IsError)
        {
            return res.ToValidationProblem();
        }


        return Ok(res.Value);
    }

    /// <summary>
    /// Changes the menuitem with the given id
    /// </summary>
    /// <param name="menuItemId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{menuItemId:int}")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<ActionResult<MenuItemVM>> PutMenuItemById(int menuItemId, UpdateMenuItemRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var res = await service.PutMenuItemByIdAsync(user, menuItemId, request);

        if (res.IsError)
        {
            return res.ToValidationProblem();
        }

        return Ok(res.Value);
    }
    /// <summary>
    /// Deletes chosen menu item
    /// </summary>
    /// <param name="menuItemId">id of the menu item to delete</param>
    /// <returns></returns>
    [HttpDelete("{menuItemId:int}")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(204), ProducesResponseType(400)]
    public async Task<ActionResult> DeleteMenuItemByIdAsync(int menuItemId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var res = await service.DeleteMenuItemByIdAsync(menuItemId, user);

        if (res.IsError)
        {
            return res.ToValidationProblem();
        }

        return NoContent();
    }

}
