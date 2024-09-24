#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Reservant.Api.Dtos;

public class WeatherForecastVM
{
    public required DateOnly Date { get; init; }

    public required int TemperatureC { get; init; }

    public required string? Summary { get; init; }

    public required int TemperatureF { get; init; }
}
