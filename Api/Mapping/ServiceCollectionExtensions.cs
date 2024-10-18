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
        AddAutoMapper(services);
    }

    /// <summary>
    /// Configure and add AutoMapper to the service collection
    /// </summary>
    private static void AddAutoMapper(IServiceCollection services)
    {
        var profiles = typeof(ServiceCollectionExtensions).Assembly
            .DefinedTypes
            .Where(t => t.IsAssignableTo(typeof(Profile)));
        foreach (var profile in profiles)
        {
            services.AddSingleton(typeof(Profile), profile);
        }

        var mappingServiceProvider = services.BuildServiceProvider(validateScopes: true);
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfiles(mappingServiceProvider.GetServices<Profile>());
        });
    }
}
