namespace Reservant.Api.Push;

/// <summary>
/// Extension methods to help register push services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register push-message services
    /// </summary>
    public static void AddPushServices(this IServiceCollection services)
    {
        services.AddSingleton<PushService>();
        services.AddScoped<PushMiddleware>();
    }

    /// <summary>
    /// Use middleware for push messages
    /// </summary>
    public static void UsePushMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<PushMiddleware>();
    }
}
