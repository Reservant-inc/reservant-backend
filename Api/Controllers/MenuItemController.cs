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
public class MenuItemController(UserManager<User> userManager, MenuItemsService service): Controller
{

    /// <summary>
    /// Creates menu items in the given restaurant
    /// </summary>
    /// <param name="menuItems">Items to be created</param>
    /// <returns>The created list of menuItems</returns>
    [HttpPost]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(201), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<ActionResult<List<MenuItemVM>>> CreateMenuItems(CreateMenuItemRequest menuItems)
    {
        var user = await userManager.GetUserAsync(User);

        var res = await service.CreateMenuItemsAsync(user!, menuItems);

        if (res.IsError)
        {
            return res.ToValidationProblem();
        }

        return Created("", res.Value);
    }


    /// <summary>
    /// Gets menu item by given id
    /// </summary>
    /// <param name="itemId">Id of the menuItem</param>
    /// <returns>The found menu item</returns>
    [HttpGet]
    [Route("{itemId:int}")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<ActionResult<MenuItemVM>> GetMenuItemById(int itemId)
    {
        var user = await userManager.GetUserAsync(User);

        var res = await service.GetMenuItemByIdAsync(user!, itemId);

        if (res.IsError)
        {
            return res.ToValidationProblem();
        }


        return Ok(res.Value);
    }

    /// <summary>
    /// Changes the menuitem with the given id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<ActionResult<MenuItemVM>> PutMenuItemById(int id, UpdateMenuItemRequest request)
    {
        var user = await userManager.GetUserAsync(User);

        var res = await service.PutMenuItemByIdAsync(user!, id, request);

        if (res.IsError)
        {
            return res.ToValidationProblem();
        }

        return Ok(res.Value);
    }

}
