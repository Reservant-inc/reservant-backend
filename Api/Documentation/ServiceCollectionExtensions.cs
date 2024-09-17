using ErrorCodeDocs.SwaggerGen;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Reservant.Api.Documentation;

/// <summary>
/// Extension methods to help registering documentation-related services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register and configure Swagger services
    /// </summary>
    public static void AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            var assembly = Assembly.GetExecutingAssembly();
            var buildTime = assembly.GetCustomAttribute<BuildTimeAttribute>()?.BuildTime;

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Reservant API",
                Version = "v1",
                Description = $"""
                    Sekude

                    *Built at {buildTime:HH:mm:ss, d MMM yyyy}*
                    """
            });

            var filePath = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
            options.IncludeXmlComments(filePath, includeControllerXmlComments: true);

            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                Description = "Your JWT Token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = "JWT Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            };

            options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtSecurityScheme, new List<string>() }
            });

            options.AddOperationFilterInstance(new AuthorizationOperationFilter());
            options.IncludeErrorCodes(Assembly.GetExecutingAssembly());
        });
    }
}
