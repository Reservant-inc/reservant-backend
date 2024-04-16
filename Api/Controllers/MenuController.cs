using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Models.Dtos.Menu;
using Reservant.Api.Models.Dtos.MenuItem;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Menu item controller.
/// </summary>
[ApiController, Route("/my-restaurants")]
public class MenuController(RestaurantMenuService service) : Controller
{
    
    /// <summary>
    /// Get list of menus by given restaurant id
    /// </summary>
    /// <param name="id">Id of the restaurant.</param>
    /// <returns></returns>
    [HttpGet("{id:int}/menus")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<List<MenuVM>>> GetMenusById(int id)
    {
        var result = await service.GetMenusAsync(id);
        
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
    [ProducesResponseType(404)]
    public async Task<ActionResult<List<MenuVM>>> GetSingleMenuById(int restaurantId, int menuId)
    {
        var result = await service.GetSingleMenuAsync(restaurantId, menuId);
        
        return Ok(result);
    }

    /// <summary>
    /// Posts menu to the restaurant
    /// </summary>
    /// <param name="req">Request for Menu to be created.</param>
    /// <returns></returns>
    [HttpPost("{restaurantId:int}/menus")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> CreateMenu(int restaurantId, CreateMenuRequest req)
    {
        var result = await service.PostMenuToRestaurant(restaurantId, req);

        if (!result.IsError) return Ok(result.Value);

        ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
        return ValidationProblem();

    }
    
}