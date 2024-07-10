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
public class RestaurantTagsController(ApiDbContext context, FileUploadService uploadService) : StrictController
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
    /// Gets all validated restaurants with the given tag
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("{tag}/restaurants")]
    [ProducesResponseType(200), ProducesResponseType(404)]
    public async Task<ActionResult<List<RestaurantSummaryVM>>> GetRestaurantsWithTag(string tag)
    {

        var resultTag = await context.RestaurantTags.FirstOrDefaultAsync(t => t.Name == tag);

        if (resultTag == null)
        {
            return NotFound();
        }

        var result = await context.Restaurants
            .Where(r => r.Tags.Contains(resultTag) && r.VerifierId != null)
            .Include(r => r.Tags)
            .Include(r => r.Reviews)
            .ToListAsync();

        return Ok(result.Select(r => new RestaurantSummaryVM()
        {
            RestaurantId = r.Id,
            Name = r.Name,
            Nip = r.Nip,
            RestaurantType = r.RestaurantType,
            Address = r.Address,
            City = r.City,
            Location = new Geolocation()
            {
                Longitude = r.Location.Y,
                Latitude = r.Location.X
            },
            GroupId = r.GroupId,
            Logo = uploadService.GetPathForFileName(r.LogoFileName),
            Description = r.Description,
            ReservationDeposit = r.ReservationDeposit,
            ProvideDelivery = r.ProvideDelivery,
            Tags = r.Tags.Select(t => t.Name).ToList(),
            IsVerified = r.VerifierId != null,
            Rating = r.Rating,
            NumberReviews = r.Reviews.Count
        }).ToList());
    }
}
