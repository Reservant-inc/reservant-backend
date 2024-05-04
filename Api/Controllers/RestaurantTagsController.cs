using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Restaurant;

namespace Reservant.Api.Controllers;

/// <summary>
/// Restaurant tag management
/// </summary>
[ApiController, Route("/restaurant-tags")]
public class RestaurantTagsController(ApiDbContext context) : Controller
{
    /// <summary>
    /// Get all available tags
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<string>>> GetAll()
    {
        return Ok(await context.RestaurantTags.Select(rt => rt.Name).ToListAsync());
    }

    /// <summary>
    /// Gets all restaurants with the given tag
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("{tag:string}/restaurants")]
    public async Task<ActionResult<List<RestaurantSummaryVM>>> GetRestaurantsWithTag(string tag)
    {
    }
}
