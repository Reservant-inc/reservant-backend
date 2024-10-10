using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Reservant.Api.Services;

/// <summary>
/// Extension methods to help registering business services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register business services
    /// </summary>
    public static void AddBusinessServices(this IServiceCollection services)
    {
        var types = typeof(ServiceCollectionExtensions).Assembly
            .GetTypes().Where(t => t.Name.EndsWith("Service", StringComparison.InvariantCulture));
        foreach (var type in types)
        {
            services.TryAddScoped(type);
        }
    }
}
