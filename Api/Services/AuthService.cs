using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Reservant.Api.Models;
using Reservant.Api.Configuration;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Reservant.Api.Dtos.Auth;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;
using ValidationFailure = FluentValidation.Results.ValidationFailure;

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
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
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

        /// <summary>
        /// Change a user's password
        /// </summary>
        [ErrorCode(nameof(dto.OldPassword), ErrorCodes.IncorrectPassword)]
        [ErrorCode(nameof(dto.NewPassword), ErrorCodes.IdentityError, "Invalid new password")]
        public async Task<Result> ChangePassword(ChangePasswordRequest dto, Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString()) ??
                       throw new InvalidOperationException("User authenticated but cannot be found");
            var result = await userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
            if (result.Succeeded)
            {
                return Result.Success;
            }

            if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(dto.OldPassword),
                    ErrorCode = ErrorCodes.IncorrectPassword,
                    ErrorMessage = "Incorrect password",
                };
            }

            return ValidationUtils.AsValidationErrors(nameof(dto.NewPassword), result);
        }
    }
}
