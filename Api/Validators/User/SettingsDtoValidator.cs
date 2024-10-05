using FluentValidation;
using Reservant.Api.Dtos.User;

namespace Reservant.Api.Validators.User;

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
