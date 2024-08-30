using LogsViewer.Data;
using LogsViewer.Logger;
using LogsViewer.Viewer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LogsViewer;

/// <summary>
/// Extension methods for adding the logs viewer conviniently
/// </summary>
public static class AppBuilderExtensions
{
    /// <summary>
    /// Register necessary services for the logs viewer to work
    /// </summary>
    public static IServiceCollection AddLogsViewer(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddHttpLogging(o =>
        {
            o.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
        });
        services.AddDbContextFactory<LogDbContext>(o =>
        {
            o.UseSqlite("Data Source=log.db");
            o.UseLoggerFactory(new LoggerFactory());
        });
        services.AddScoped<LogsViewerUIService>();
        services.AddScoped<LogsViewerUIMiddleware>();
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
