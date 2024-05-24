using System.ComponentModel.DataAnnotations;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Models;

public class WeatherForecast
{
    [Key]
    public DateOnly Date { get; init; }

    [Range(-100, 100)]
    public int TemperatureC { get; init; }

    [StringLength(20)]
    public string? Summary { get; init; }
}
