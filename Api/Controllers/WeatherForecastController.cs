using Microsoft.AspNetCore.Mvc;

namespace Reservant.Api.Controllers;

/// <inheritdoc />
[ApiController, Route("/weatherforecast")]
public class WeatherForecastController : Controller
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }

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
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    Summaries[Random.Shared.Next(Summaries.Length)]
                ))
            .ToArray());
    }
}
