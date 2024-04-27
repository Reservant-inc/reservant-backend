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
    
}