using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Restaurant;
using Reservant.Api.Services;

namespace Reservant.Api.Controllers;

/// <summary>
/// Restaurant tag management
/// </summary>
[ApiController, Route("/restaurant-tags")]
public class RestaurantTagsController(ApiDbContext context, FileUploadService uploadService) : Controller
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
    [Route("{tag}/restaurants")]
    public async Task<ActionResult<List<RestaurantSummaryVM>>> GetRestaurantsWithTag(string tag)
    {
        var result = await context.Restaurants
            .Where(r => r.Tags != null && r.Tags.Select(t => t.Name).Contains(tag))
            .Include(r => r.Tags)
            .ToListAsync();

        return Ok(result.Select(r => new RestaurantSummaryVM()
        {
            Id = r.Id,
            Name = r.Name,
            Nip = r.Nip,
            RestaurantType = r.RestaurantType,
            Address = r.Address,
            City = r.City,
            GroupId = r.GroupId,
            Logo = uploadService.GetPathForFileName(r.LogoFileName),
            Description = r.Description,
            ProvideDelivery = r.ProvideDelivery,
            Tags = (r.Tags ?? []).Select(t => t.Name).ToList(),
            IsVerified = r.VerifierId != null
        }));
    }
}
