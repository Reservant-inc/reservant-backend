using AutoMapper;

namespace Reservant.Api.Mapping;

/// <summary>
/// Extension methods to help registering mapping services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register mapping services
    /// </summary>
    public static void AddMappingServices(this IServiceCollection services)
    {
        services.AddSingleton<UrlService>();
    }
}
