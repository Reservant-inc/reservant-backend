using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Models.Dtos.Menu;
using Reservant.Api.Models.Dtos.MenuItem;
using Reservant.Api.Services;

namespace Reservant.Api.Controllers;

/// <summary>
/// Menu item controller.
/// </summary>
[ApiController, Route("/my-restaurants")]
public class MyMenuItemController(RestaurantMenuService service) : Controller
{

    
    /// <summary>
    /// Get list of menus by given restaurant id
    /// </summary>
    /// <param name="id">Id of the restaurant.</param>
    /// <returns></returns>
    [HttpGet("{id}/menus")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<List<MenuVM>>> GetMenusById(int id)
    {
        var result = await service.GetMenusById(id);
        
        return Ok(result);
    }
    
}