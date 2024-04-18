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
[ApiController, Route("/my-restaurants")]
public class MenuController(RestaurantMenuService service, UserManager<User> userManager) : Controller
{
    
    /// <summary>
    /// Get list of menus by given restaurant id
    /// </summary>
    /// <param name="id">Id of the restaurant.</param>
    /// <returns></returns>
    [HttpGet("{id:int}/menus")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<List<MenuSummaryVM>>> GetMenusById(int id)
    {
        var result = await service.GetMenusAsync(id);
        
        if (result.IsNullOrEmpty()) return NotFound($"Menus with id {id} not found.");
        
        return Ok(result);
    }
    
    
    /// <summary>
    /// Gets a single menu with details for a given menu ID and restaurant ID.
    /// </summary>
    /// <param name="restaurantId">The ID of the restaurant.</param>
    /// <param name="menuId">The ID of the menu.</param>
    /// <returns></returns>
    [HttpGet("{restaurantId:int}/menus/{menuId:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<MenuVM>> GetSingleMenuById(int restaurantId, int menuId)
    {
        var result = await service.GetSingleMenuAsync(restaurantId, menuId);
        
        if (!result.IsError) return Ok(result.Value);
        
        ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
        return ValidationProblem();
    }

    /// <summary>
    /// Posts menu to the restaurant
    /// </summary>
    /// <param name="restaurantId">Restaurant id.</param>
    /// <param name="req">Request for Menu to be created.</param>
    /// <returns></returns>
    [HttpPost("{restaurantId:int}/menus")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> CreateMenu(int restaurantId, CreateMenuRequest req)
    {
        var user = await userManager.GetUserAsync(User);
        var result = await service.PostMenuToRestaurant(restaurantId, req, user);

        if (!result.IsError) return Ok(result.Value);

        ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
        return ValidationProblem();
    }

    /// <summary>
    /// Adds a given menuItem ids to specific menu
    /// </summary>
    /// <param name="id">Id of menu</param>
    /// <param name="request">Request containing MenuItemIds</param>
    /// <returns>The created list of menuItems</returns>
    [HttpPost("/menu/{id:int}/items")]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)] 
    [ProducesResponseType(401)]
    public async Task<ActionResult> AddToMenu(int id, AddItemsRequest request)
    {
        var user = await userManager.GetUserAsync(User);

        var result = await service.AddItemsToMenuAsync(id, request, user);
        
        if (!result.IsError) return Ok(result.Value);
        
        ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
        return ValidationProblem();
    }
    
    
}