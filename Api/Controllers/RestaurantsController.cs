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
/// Restaurants from the perspective of other people than the owner
/// </summary>
[ApiController, Route("/restaurants")]
public class RestaurantController(UserManager<User> userManager, RestaurantService service) : Controller
{

    /// <summary>
    /// Verify restaurant
    /// </summary>
    /// <remarks>
    /// For CustomerSupportAgent. Sets the restaurant's verifier ID to the current user's ID.
    /// </remarks>
    /// <param name="restaurantId">ID of the restaurant</param>
    /// <response code="400">Restaurant already verified</response>
    [HttpPost("{restaurantId:int}/verify")]
    [ProducesResponseType(200), ProducesResponseType(404),ProducesResponseType(400)]
    [Authorize(Roles = Roles.CustomerSupportAgent)]
    public async Task<ActionResult> SetVerifiedId(int restaurantId)
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var result = await service.SetVerifiedIdAsync(user, restaurantId);

        switch (result)
        {
            case VerificationResult.VerifierSetSuccessfully:
                return Ok();
            case VerificationResult.VerifierAlreadyExists:
                return BadRequest();
            case VerificationResult.RestaurantNotFound:
                return NotFound();
            default:
                return null;
        }
    }

}
