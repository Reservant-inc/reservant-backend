#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Reservant.Api.Dtos;

public class WeatherForecastVM
{
    public DateOnly Date { get; init; }

    public int TemperatureC { get; init; }

    public string? Summary { get; init; }

    public int TemperatureF { get; init; }
}
