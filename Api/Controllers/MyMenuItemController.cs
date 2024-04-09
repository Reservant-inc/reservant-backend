using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.MenuItem;
using Reservant.Api.Services;

namespace Reservant.Api.Controllers;

[ApiController, Route("/my-restaurant-groups/{restaurantId}/menu-items")]
[Authorize(Roles = Roles.RestaurantOwner)]
public class MyMenuItemController(UserManager<User> userManager, MenuItemsService service): Controller
{
    [HttpPost]
    [ProducesResponseType(201), ProducesResponseType(401), ProducesResponseType(400)]
    public async Task<ActionResult> CreateMenuItems(int restaurantId, List<CreateMenuItemRequest> menuItems)
    {
        var user = await userManager.GetUserAsync(User);

        var res = await service.CreateMenuItemsAsync(user!, restaurantId, menuItems);

        if (res.IsError)
        {
            return BadRequest();
        }

        return Created();
    }

}
