using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reservant.LogsViewer.Data;
using Reservant.LogsViewer.Logger;
using Reservant.LogsViewer.Viewer;

namespace Reservant.LogsViewer;

/// <summary>
/// Extension methods for adding the logs viewer conviniently
/// </summary>
public static class AppBuilderExtensions
{
    /// <summary>
    /// Register necessary services for the logs viewer to work
    /// </summary>
    public static IServiceCollection AddLogsViewer(
        this IServiceCollection services, Action<DbContextOptionsBuilder> configureDbContext)
    {
        services.AddHttpContextAccessor();
        services.AddHttpLogging(o =>
        {
            o.LoggingFields = HttpLoggingFields.All | HttpLoggingFields.RequestQuery;
        });
        services.AddDbContextFactory<LogDbContext>(o =>
        {
            o.UseLoggerFactory(new LoggerFactory());
            configureDbContext(o);
        });
        services.AddScoped<LogsViewerUIService>();
        return services;
    }

    /// <summary>
    /// Register the logs viewer logger provider
    /// </summary>
    public static void RegisterLogsViewerProvider(this IServiceProvider services)
    {
        var httpAccessor = services.GetRequiredService<IHttpContextAccessor>();
        var dbContextFactory = services.GetRequiredService<IDbContextFactory<LogDbContext>>();
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        loggerFactory.AddProvider(new SqliteLoggerProvider(httpAccessor, dbContextFactory));
    }

    /// <summary>
    /// Add middleware that shows the logs viewer UI
    /// </summary>
    public static void UseLogsViewerUI(this IApplicationBuilder app)
    {
        app.UseMiddleware<LogsViewerUIMiddleware>();
    }
}
