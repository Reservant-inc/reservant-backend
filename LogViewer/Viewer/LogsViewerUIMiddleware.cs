using Microsoft.AspNetCore.Http;

namespace LogsViewer.Viewer;

/// <summary>
/// Middleware that shows the logs viewer UI
/// </summary>
internal class LogsViewerUIMiddleware(LogsViewerUIService ui) : IMiddleware
{
    /// <inheritdoc/>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (HttpMethods.IsGet(context.Request.Method) && context.Request.Path == "/logs.html")
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(await ui.RenderLogsPage());
            return;
        }

        await next(context);
    }
}
