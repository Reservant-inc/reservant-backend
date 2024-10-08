using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Dtos.Ingredient;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Manage ingredients
/// </summary>
[ApiController, Route("/ingredients")]
public class IngredientsController(
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
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await ingredientService.CreateIngredientAsync(request, userId.Value);
        return OkOrErrors(result);
    }


    /// <summary>
    /// Update an existing ingredient
    /// </summary>
    /// <param name="ingredientId">ID of the ingredient to update</param>
    /// <param name="request">Ingredient update request</param>
    /// <returns>Updated ingredient information</returns>
    [HttpPut("{ingredientId:int}")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [Authorize(Roles = $"{Roles.RestaurantOwner},{Roles.RestaurantEmployee}")]
    [MethodErrorCodes<IngredientService>(nameof(IngredientService.UpdateIngredientAsync))]
    public async Task<ActionResult<IngredientVM>> UpdateIngredient(int ingredientId, [FromBody] UpdateIngredientRequest request)
    {
        var userId = User.GetUserId()!;
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await ingredientService.UpdateIngredientAsync(ingredientId, request, userId.Value);
        return OkOrErrors(result);
    }
}
