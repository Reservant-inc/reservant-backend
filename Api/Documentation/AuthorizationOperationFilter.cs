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

        var endpointRoles = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .SelectMany(a => a.Roles?.Split(",") ?? []);
        foreach (var role in endpointRoles)
        {
            roles.Add(role);
        }

        if (roles.Count == 0)
        {
            var controllerRoles = context.MethodInfo.DeclaringType?
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .SelectMany(a => a.Roles?.Split(",") ?? []);

            if (controllerRoles is not null)
            {
                foreach (var role in controllerRoles)
                {
                    roles.Add(role);
                }
            }
        }

        if (roles.Count == 0)
        {
            return;
        }

        var rolesStr = string.Join(", ", roles);
        operation.Description += $"<p><b>Required Roles:</b> {rolesStr}</p>";
    }
}
