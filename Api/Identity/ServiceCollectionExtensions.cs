using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Options;

namespace Reservant.Api.Identity;

/// <summary>
/// Extension methods to help with registering authentication and
/// authorization services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register authentication and authorization services
    /// </summary>
    public static void AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                var jwtOptions = configuration.GetSection(JwtOptions.ConfigSection)
                    .Get<JwtOptions>() ?? throw new InvalidOperationException("Failed to read JwtOptions");

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtOptions.GetKeyBytes()),
                    ValidateIssuerSigningKey = true
                };

                o.Events = new JwtBearerEvents();
                o.Events.OnTokenValidated += context =>
                {
                    context.HttpContext.Items.Add(
                        HttpContextItems.AuthExpiresUtc, context.Properties.ExpiresUtc);
                    return Task.CompletedTask;
                };
            });

        services.AddAuthorization();
        services
            .AddIdentityCore<User>(o =>
            {
                o.SignIn.RequireConfirmedEmail = false;
                o.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApiDbContext>()
            .AddDefaultTokenProviders();
    }
}
