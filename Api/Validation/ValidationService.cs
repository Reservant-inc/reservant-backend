using FluentValidation;
using FluentValidation.Results;

namespace Reservant.Api.Validation;

/// <summary>
/// Convenient way to validate objects using FluentValidation
/// </summary>
public class ValidationService(IServiceProvider serviceProvider)
{
    /// <summary>
    /// Validate <paramref name="instance"/> using dedicated validator
    /// </summary>
    /// <param name="instance">Object to validate</param>
    /// <typeparam name="T">Type of the object to validate</typeparam>
    /// <param name="userId">ID of the logged in user</param>
    /// <returns><see cref="ValidationResult"/></returns>
    public async Task<ValidationResult> ValidateAsync<T>(T instance, string? userId)
    {
        var validationContext = new ValidationContext<T>(instance);
        validationContext.RootContextData["UserId"] = userId;

        return await serviceProvider
            .GetRequiredService<IValidator<T>>()
            .ValidateAsync(validationContext);
    }
}
