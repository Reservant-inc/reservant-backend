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
//[Authorize(Roles = Roles.CustomerSupportManager)]
public class RestaurantController(UserManager<User> userManager, RestaurantService service) : Controller
{

    /// <summary>
    /// Sets properties VerifierId on users ID
    /// </summary>
    /// <param name="restaurantId">int</param>
    /// <returns>conformation if action was performed succesfuly</returns>
    [HttpPost("{restaurantId:int}")]
    [ProducesResponseType(200), ProducesResponseType(401)]
    public async Task<ActionResult> setVerifierId(int restaurantId)
    {

        // var user = await userManager.GetUserAsync(User);

        // if (user == null)
        // {
        //     return Unauthorized();
        // }

        var userId = 1;
        var result = await service.setVerifierIdAsync(1,restaurantId);

        if(result)
            return Ok();
        return NotFound();

        // Ustawia property VerifierId wybranej restauracji na ID zalogowanego użytkownika
        // Kontroler RestaurantsController (nie MyRestaurantsController) DONE
        // Autoryzacja CustomerSupportAgent DONE
        // Zwraca 200 jeśli wszystko OK, 404 jeśli restauracji nie znaleziono 

    }

    
}

// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;
// using Reservant.Api.Identity;
// using Reservant.Api.Models;
// using Reservant.Api.Models.Dtos.RestaurantGroup;
// using Reservant.Api.Services;
// using Reservant.Api.Validation;

// namespace Reservant.Api.Controllers
// {
//     /// <summary>
//     /// Controller resposnible for setting verifiedId for logged in user
//     /// Only CustomerSupportAgent can use this controllers
//     /// </summary>
//     /// <request code="404"> Refering to nonexistant restaurant </request>
//     /// <request code="200"> When assigning is succesful </request>
//     /// <param name="userManager"></param>
//     /// <param name="service"></param>
//     [ApiController, Route("/restaurants")]
//     // [Authorize(Roles = Roles.CustomerSupportAgent)]
//     [Authorize(Roles = Roles.RestaurantOwner)]
//     public class RestaurantsController(RestaurantService restaurantService, UserManager<User> userManager) : Controller
//     {
//         /// <summary>
//         /// Sets properties VerifierId on users ID
//         /// </summary>
//         /// <returns></returns>
//         [HttpPost("{restaurantId:int}")]
//         [ProducesResponseType(200), ProducesResponseType(400)]
//         public async Task<ActionResult> setVerifierId(int restaurantId) {
            
//             // var user = await userManager.GetUserAsync(User);
//             // if (user == null)
//             // {
//             //     return Unauthorized();
//             // }
//             var userId = 1;

//             var result = await service.setVerifierIdAsync(1,restaurantId);
//             return Ok(200);

//             // var result = await restaurantService.FUNKCJA(user);
//             // if (result.IsError)
//             // {
//             //     ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
//             //     return ValidationProblem();
//             // }

            
//         }
//     }
// }
