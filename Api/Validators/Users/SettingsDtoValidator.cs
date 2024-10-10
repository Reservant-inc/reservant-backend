using FluentValidation;
using Reservant.Api.Dtos.Users;

namespace Reservant.Api.Validators.Users;

/// <summary>
/// Validator for SettingsDto
/// </summary>
public class SettingsDtoValidator : AbstractValidator<SettingsDto>
{
    /// <inheritdoc />
    public SettingsDtoValidator()
    {
        RuleFor(x => x.Language)
            .CultureInfoString();
    }
}
