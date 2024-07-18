using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models.Dtos.Location;
using Reservant.Api.Models.Dtos.Restaurant;
using Reservant.Api.Services;

namespace Reservant.Api.Controllers;

/// <summary>
/// Restaurant tag management
/// </summary>
[ApiController, Route("/restaurant-tags")]
public class RestaurantTagsController(ApiDbContext context) : StrictController
{
    /// <summary>
    /// Get all available tags
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<string>>> GetAll()
    {
        return Ok(await context.RestaurantTags.Select(rt => rt.Name).ToListAsync());
    }
}
