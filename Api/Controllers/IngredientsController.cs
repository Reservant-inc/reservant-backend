using ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.Ingredient;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Manage ingredients
/// </summary>
[ApiController, Route("/ingredients")]
public class IngredientsController(
    UserManager<User> userManager,
    IngredientService ingredientService
    ) : StrictController
{
    /// <summary>
    /// Add a new ingredient to the system
    /// </summary>
    /// <param name="request">Ingredient creation request</param>
    /// <returns>Created ingredient information</returns>
    [HttpPost]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = Roles.RestaurantOwner)]
    [MethodErrorCodes<IngredientService>(nameof(IngredientService.CreateIngredientAsync))]
    public async Task<ActionResult<IngredientVM>> CreateIngredient([FromBody] CreateIngredientRequest request)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await ingredientService.CreateIngredientAsync(request, userId);
        return OkOrErrors(result);
    }
}