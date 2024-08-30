using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LogsViewer.Viewer;

/// <summary>
/// Middleware that shows the logs viewer UI
/// </summary>
internal class LogsViewerUIMiddleware
{
    private const string PathBase = "/logs";

    private static readonly string EmbeddedFilesBaseNamespace =
        typeof(LogsViewerUIMiddleware).Assembly.GetName().Name! + ".Resources";

    private readonly StaticFileMiddleware _staticFileMiddleware;

    public LogsViewerUIMiddleware(
        RequestDelegate next,
        IWebHostEnvironment hostingEnv,
        ILoggerFactory loggerFactory)
    {
        var staticFileOptions = new StaticFileOptions
        {
            RequestPath = PathBase,
            FileProvider = new EmbeddedFileProvider(
                typeof(LogsViewerUIMiddleware).Assembly, EmbeddedFilesBaseNamespace),
        };
        _staticFileMiddleware = new(next, hostingEnv, Options.Create(staticFileOptions), loggerFactory);
    }

    /// <inheritdoc/>
    public async Task InvokeAsync(HttpContext context, LogsViewerUIService ui)
    {
        var isGet = HttpMethods.IsGet(context.Request.Method);

        if (isGet && context.Request.Path == PathBase)
        {
            context.Response.StatusCode = 301;
            context.Response.Headers["Location"] = PathBase + "/index.html";
            return;
        }

        if (isGet && context.Request.Path == PathBase + "/index.html")
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(await ui.RenderLogsPage());
            return;
        }

        await _staticFileMiddleware.Invoke(context);
    }
}
