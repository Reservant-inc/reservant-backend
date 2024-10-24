﻿using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos.MenuItems;
using Reservant.Api.Identity;
using Reservant.Api.Models;
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
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(401)]
    [MethodErrorCodes<MenuItemsService>(nameof(MenuItemsService.CreateMenuItemsAsync))]
    public async Task<ActionResult<MenuItemVM>> CreateMenuItems(CreateMenuItemRequest menuItem)
    {
        var userId = User.GetUserId();

        var res = await service.CreateMenuItemsAsync(userId!.Value, menuItem);
        return OkOrErrors(res);
    }


    /// <summary>
    /// Gets menu item by given id
    /// </summary>
    /// <param name="menuItemId">Id of the menuItem</param>
    /// <returns>The found menu item</returns>
    [HttpGet]
    [Route("{menuItemId:int}")]
    [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(401)]
    [MethodErrorCodes<MenuItemsService>(nameof(MenuItemsService.GetMenuItemByIdAsync))]
    public async Task<ActionResult<MenuItemVM>> GetMenuItemById(int menuItemId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var res = await service.GetMenuItemByIdAsync(user, menuItemId);
        return OkOrErrors(res);
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
    [MethodErrorCodes<MenuItemsService>(nameof(MenuItemsService.PutMenuItemByIdAsync))]
    public async Task<ActionResult<MenuItemVM>> PutMenuItemById(int menuItemId, UpdateMenuItemRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var res = await service.PutMenuItemByIdAsync(user, menuItemId, request);
        return OkOrErrors(res);
    }


    /// <summary>
    /// Deletes chosen menu item
    /// </summary>
    /// <param name="menuItemId">id of the menu item to delete</param>
    /// <returns></returns>
    [HttpDelete("{menuItemId:int}")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(204), ProducesResponseType(400)]
    [MethodErrorCodes<MenuItemsService>(nameof(MenuItemsService.DeleteMenuItemByIdAsync))]
    public async Task<ActionResult> DeleteMenuItemByIdAsync(int menuItemId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var res = await service.DeleteMenuItemByIdAsync(menuItemId, user);
        return OkOrErrors(res);
    }

}
