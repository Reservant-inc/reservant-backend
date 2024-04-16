using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.RestaurantGroup;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;


/// <summary>
/// Controller resposnible for setting verifiedId for logged in user
/// Only CustomerSupportAgent can use this controllers
/// </summary>
/// <request code="404"> Refering to nonexistant restaurant </request>
/// <request code="200"> When assigning is succesful </request>
/// <param name="userManager"></param>
/// <param name="service"></param>
[ApiController, Route("/restaurants")]
public class RestaurantController(UserManager<User> userManager, RestaurantService service) : Controller
{

    /// <summary>
    /// Sets properties VerifierId on users ID
    /// </summary>
    /// <param name="restaurantId">int</param>
    /// <returns>conformation if action was performed succesfuly</returns>
    [HttpPost("{restaurantId:int}/verify")]
    [ProducesResponseType(200), ProducesResponseType(404)]
    [Authorize(Roles.CustomerSupportAgent)]
    public async Task<ActionResult> SetVerifierId(int restaurantId)
    {

        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.SetVerifierIdAsync(user,restaurantId);

        if(result)
            return Ok();
        return NotFound();
    }
}
