using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Reservant.Api.Models;
using Reservant.Api.Options;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;

namespace Reservant.Api.Services
{
    /// <summary>
    /// Service responsible for managing user authentication
    /// </summary>
    public class AuthService(UserManager<User> userManager, IOptions<JwtOptions> jwtOptions)
    {
        /// <summary>
        /// Generate a new JWT
        /// </summary>
        /// <param name="user">The user to create the token for</param>
        public SecurityToken GenerateSecurityToken(User user)
        {
            JwtSecurityTokenHandler handler = new();
            var claims = new List<Claim>
            {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(ClaimTypes.Name, user.UserName!)
            };

            var roles = userManager.GetRolesAsync(user).Result;
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TimeSpan.FromHours(jwtOptions.Value.LifetimeHours)),
                Issuer = jwtOptions.Value.Issuer,
                Audience = jwtOptions.Value.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(jwtOptions.Value.GetKeyBytes()),
                    SecurityAlgorithms.HmacSha256)
            };

            var token = handler.CreateToken(tokenDescriptor);
            return token;
        }


    }
}
