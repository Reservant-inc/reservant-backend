using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;

namespace Reservant.Api.Controllers;

/// <inheritdoc />
[ApiController, Route("/weatherforecast")]
public class WeatherForecastController(ApiDbContext context) : Controller
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private static int ToFahrenheit(int celsius)
    {
        return 32 + (int)(celsius / 0.5556);
    }

    /// <summary>
    /// Return all the forecasts.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    [Authorize]
    public async Task<ActionResult<List<WeatherForecast>>> GetAll()
    {
        return Ok(await context.WeatherForecasts
            .Select(f => new WeatherForecastVM
            {
                Date = f.Date,
                TemperatureC = f.TemperatureC,
                Summary = f.Summary,
                TemperatureF = ToFahrenheit(f.TemperatureC)
            })
            .ToListAsync());
    }
}
