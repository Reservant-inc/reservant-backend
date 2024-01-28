using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Models;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Reservant.Api.Data;

public class ApiDbContext(DbContextOptions<ApiDbContext> options) : IdentityDbContext<User>(options)
{
    public required DbSet<WeatherForecast> WeatherForecasts { get; init; }
}
