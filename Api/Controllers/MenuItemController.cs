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

[ApiController, Route("/my-restaurants/{restaurantId:int}/menu-items")]
[Authorize(Roles = Roles.RestaurantOwner)]
public class MenuItemController(UserManager<User> userManager, MenuItemsService service): Controller
{

    /// <summary>
    /// Creates menu items in the given restaurant
    /// </summary>
    /// <param name="restaurantId">Id of the restaurant in which menu items will be created</param>
    /// <param name="menuItems">Items to be created</param>
    /// <returns>The created list of menuItems</returns>
    [HttpPost]
    [ProducesResponseType(201), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<ActionResult<List<MenuItemVM>>> CreateMenuItems(int restaurantId, List<CreateMenuItemRequest> menuItems)
    {
        var user = await userManager.GetUserAsync(User);

        var res = await service.CreateMenuItemsAsync(user!, restaurantId, menuItems);

        if (res.IsError)
        {
            ValidationUtils.AddErrorsToModel(res.Errors!, ModelState);
            return ValidationProblem();
        }

        return Created("", res.Value);
    }


    /// <summary>
    /// Gets menu items from the given restaurant
    /// </summary>
    /// <param name="restaurantId"></param>
    /// <returns>The found list of menuItems</returns>
    [HttpGet]
    [ProducesResponseType(201), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<ActionResult<MenuItemVM>> GetMenuItems(int restaurantId)
    {
        var user = await userManager.GetUserAsync(User);

        var res = await service.GetMenuItemsAsync(user!, restaurantId);

        if (res.IsError)
        {
            ValidationUtils.AddErrorsToModel(res.Errors!, ModelState);
            return ValidationProblem();
        }

        return Ok(res.Value);
    }


    /// <summary>
    /// Gets menu item by given id
    /// </summary>
    /// <param name="restaurantId"></param>
    /// <param name="itemId">Id of the menuItem</param>
    /// <returns>The found menu item</returns>
    [HttpGet]
    [Route("{itemId:int}")]
    [ProducesResponseType(201), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<ActionResult<MenuItemVM>> GetMenuItemById(int restaurantId, int itemId)
    {
        var user = await userManager.GetUserAsync(User);

        var res = await service.GetMenuItemByIdAsync(user!, restaurantId, itemId);

        if (res.IsError)
        {
            ValidationUtils.AddErrorsToModel(res.Errors!, ModelState);
            return ValidationProblem();
        }


        return Ok(res.Value);
    }

}
