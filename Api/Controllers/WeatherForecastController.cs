using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Models;

namespace Reservant.Api.Controllers;

/// <inheritdoc />
[ApiController, Route("/weatherforecast")]
public class WeatherForecastController : Controller
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    /// <summary>
    /// Generate five random weather forecasts.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(WeatherForecast[]), 200)]
    public IActionResult Index()
    {
        return Ok(Enumerable.Range(1, 5)
            .Select(index =>
                new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
            .ToArray());
    }
}
