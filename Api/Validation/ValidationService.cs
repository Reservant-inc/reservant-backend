using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Models;

namespace Reservant.Api.Validation;

/// <summary>
/// Convenient way to validate objects using FluentValidation
/// </summary>
public class ValidationService(
    UserManager<User> userManager,
    IHttpContextAccessor accessor,
    IServiceProvider serviceProvider)
{
    /// <summary>
    /// Validate <paramref name="instance"/> using dedicated validator
    /// </summary>
    /// <param name="instance">Object to validate</param>
    /// <typeparam name="T">Type of the object to validate</typeparam>
    /// <returns><see cref="ValidationResult"/></returns>
    public async Task<ValidationResult> ValidateAsync<T>(T instance)
    {
        var validationContext = new ValidationContext<T>(instance);
        if (accessor.HttpContext is not null)
        {
            validationContext.RootContextData["UserId"] = userManager.GetUserId(accessor.HttpContext.User);
        }

        return await serviceProvider
            .GetRequiredService<IValidator<T>>()
            .ValidateAsync(validationContext);
    }
}
