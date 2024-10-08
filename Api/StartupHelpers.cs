using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Reservant.Api;

/// <summary>
/// Extension methods to register required services
/// </summary>
public static class StartupHelpers
{
    /// <summary>
    /// Add custom exception handler. Needed for two reasons:
    /// 1. Catches the exception before the HTTP logging middleware, and forms a correct response
    /// 2. In production, return an expected problem details JSON
    /// </summary>
    public static void AddCustomExceptionHandler<T>(this T services)
        where T : IServiceCollection
    {
        services.AddExceptionHandler(o =>
        {
            o.ExceptionHandler = async httpContext =>
            {
                var environment = httpContext.RequestServices.GetRequiredService<IHostEnvironment>();
                var problemDetailsFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                var errorInfo = httpContext.Features.GetRequiredFeature<IExceptionHandlerFeature>();

                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                if (environment.IsDevelopment())
                {
                    httpContext.Response.ContentType = "text/plain";
                    await httpContext.Response.WriteAsync(
                        errorInfo.Error.ToString());
                }
                else
                {
                    await httpContext.Response.WriteAsJsonAsync(
                        problemDetailsFactory.CreateProblemDetails(httpContext, title: "Internal server error"));
                }
            };
        });
    }
}
