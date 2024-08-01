using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Reservant.Api.Documentation;

/// <summary>
/// Adds information about required roles to Swagger
/// </summary>
public class AuthorizationOperationFilter : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var roles = new HashSet<string>();

        var endpointAuthorization = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .ToList();

        var authorizationRequired = endpointAuthorization.Count > 0;

        var endpointRoles = endpointAuthorization
            .SelectMany(a => a.Roles?.Split(",") ?? []);
        foreach (var role in endpointRoles)
        {
            roles.Add(role);
        }

        if (roles.Count == 0)
        {
            var controllerAuthorization = context.MethodInfo.DeclaringType?
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .ToList() ?? [];

            authorizationRequired  = authorizationRequired || controllerAuthorization.Count > 0;

            var controllerRoles = controllerAuthorization
                .SelectMany(a => a.Roles?.Split(",") ?? []);
            foreach (var role in controllerRoles)
            {
                roles.Add(role);
            }
        }

        if (roles.Count > 0)
        {
            var rolesStr = string.Join(", ", roles);
            operation.Description += $"\n\n**Required Roles:** {rolesStr}";
        }
        else if (authorizationRequired)
        {
            operation.Description += "\n\n**Authentication required**";
        }
    }
}
