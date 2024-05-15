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
        
        ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
        return ValidationProblem();
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

        ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
        return ValidationProblem();
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
        
        ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
        return ValidationProblem();
    }
    

    /// <summary>
    /// removes menu item from given menue by given id
    /// </summary>
    /// <param name="req">request of removal</param>
    /// <param name="id">id of the menu</param>
    /// <returns>The found menu item</returns>
    [HttpDelete]
    [Route("{id:int}/items")]
    [ProducesResponseType(200), ProducesResponseType(404), ProducesResponseType(400)]
    public async Task<ActionResult> RemoveMenuItemFromMenu(int id, RemoveItemsRequest req)
    {
        var user = await userManager.GetUserAsync(User);

        var res = await service.RemoveMenuItemFromMenuAsync(user!, id, req);

        switch (res)
        {
            case RestaurantMenuService.RemoveMenuItemResult.Success:
                return Ok();
            case RestaurantMenuService.RemoveMenuItemResult.MenuNotFound:
                return NotFound();
            case RestaurantMenuService.RemoveMenuItemResult.NoValidMenuItems:
                return BadRequest();
            default:
                return StatusCode(500, "Internal server error");
        }
    }
    
}