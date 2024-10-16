﻿using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Menus;
using Reservant.Api.Dtos.MenuItems;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Enums;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Menu item controller.
/// </summary>
[ApiController, Route("/menus")]
public class MenuController(RestaurantMenuService service, UserManager<User> userManager) : StrictController
{

    /// <summary>
    /// Gets a single menu with details for a given menu ID and restaurant ID.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{menuId:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [MethodErrorCodes<RestaurantMenuService>(nameof(RestaurantMenuService.GetSingleMenuAsync))]
    public async Task<ActionResult<MenuVM>> GetSingleMenuById(int menuId)
    {
        var result = await service.GetSingleMenuAsync(menuId);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Get list of items of a single menu
    /// </summary>
    /// <param name="menuId">ID of the menu</param>
    /// <param name="name">Search by name</param>
    /// <param name="orderBy">Sorting order</param>
    /// <param name="page">Page number</param>
    /// <param name="perPage">Items per page</param>
    /// <returns></returns>
    [HttpGet("{menuId:int}/items")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [MethodErrorCodes<RestaurantMenuService>(nameof(RestaurantMenuService.GetMenuItemsAsync))]
    public async Task<ActionResult<Pagination<MenuItemSummaryVM>>> GetMenuItems(
        int menuId, int page = 0, int perPage = 20,
        string? name = null, MenuItemSorting orderBy = MenuItemSorting.PriceDesc)
    {
        var result = await service.GetMenuItemsAsync(menuId, page, perPage, name, orderBy);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Create a Menu in a restaurant
    /// </summary>
    /// <param name="req">Request for Menu to be created.</param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [MethodErrorCodes<RestaurantMenuService>(nameof(RestaurantMenuService.PostMenuToRestaurant))]
    public async Task<ActionResult<MenuSummaryVM>> CreateMenu(CreateMenuRequest req)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await service.PostMenuToRestaurant(req, user);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Adds MenuItems to a Menu
    /// </summary>
    /// <param name="menuId">Id of menu</param>
    /// <param name="request">Request containing MenuItemIds</param>
    /// <returns>The created list of menuItems</returns>
    [HttpPost("{menuId:int}/items")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [MethodErrorCodes<RestaurantMenuService>(nameof(RestaurantMenuService.AddItemsToMenuAsync))]
    public async Task<ActionResult<MenuVM>> AddToMenu(int menuId, AddItemsRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await service.AddItemsToMenuAsync(menuId, request, user);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Update menu
    /// </summary>
    /// <param name="request">New data</param>
    /// <param name="menuId">ID of the menu</param>
    /// <returns></returns>
    [HttpPut("{menuId:int}")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [MethodErrorCodes<RestaurantMenuService>(nameof(RestaurantMenuService.UpdateMenuAsync))]
    public async Task<ActionResult<MenuVM>> UpdateMenu(UpdateMenuRequest request, int menuId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await service.UpdateMenuAsync(request, menuId, user);
        return OkOrErrors(result);
    }

    /// <summary>
    /// Delete a menu
    /// </summary>
    [HttpDelete("{menuId:int}")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(204), ProducesResponseType(400)]
    [MethodErrorCodes<RestaurantMenuService>(nameof(RestaurantMenuService.DeleteMenuAsync))]
    public async Task<ActionResult> DeleteMenu(int menuId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var res = await service.DeleteMenuAsync(menuId, user);
        return OkOrErrors(res);
    }


    /// <summary>
    /// Remove a MenuItem from a Menu
    /// </summary>
    /// <param name="req">request of removal</param>
    /// <param name="menuId">id of the menu</param>
    /// <returns>The found menu item</returns>
    [HttpDelete]
    [Route("{menuId:int}/items")]
    [ProducesResponseType(200), ProducesResponseType(404), ProducesResponseType(400)]
    [MethodErrorCodes<RestaurantMenuService>(nameof(RestaurantMenuService.RemoveMenuItemFromMenuAsync))]
    public async Task<ActionResult> RemoveMenuItemFromMenu(int menuId, RemoveItemsRequest req)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var res = await service.RemoveMenuItemFromMenuAsync(user, menuId, req);            
        return OkOrErrors(res);
    }

    /// <summary>
    /// Gets all menu types
    /// </summary>
    [HttpGet("menu-types")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    public ActionResult<List<MenuType>> GetMenuTypesAsync()
    {
        var menuTypes = Enum.GetValues<MenuType>().ToList();
        return Ok(menuTypes);
    }
}
