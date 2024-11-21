using FluentValidation;
using Reservant.Api.Dtos.Users;
using Reservant.Api.Services;

namespace Reservant.Api.Validators.Users;

/// <summary>
/// Validator for UpdateUserDetailsRequest
/// </summary>
public class UpdateUserDetailsValidator : AbstractValidator<UpdateUserDetailsRequest>
{
    /// <inheritdoc />
    public UpdateUserDetailsValidator(FileUploadService uploadService)
    {
        RuleFor(u => u.FirstName)
            .NotEmpty()
            .MaximumLength(30)
            .IsValidPersonalName();

        RuleFor(u => u.LastName)
            .NotEmpty()
            .MaximumLength(30)
            .IsValidPersonalName();

        RuleFor(r => r.PhoneNumber)
            .IsValidPhoneNumber();

        RuleFor(r => r.BirthDate)
            .NotEmpty()
            .DateInPast();

        RuleFor(u => u.Photo)
            .MaximumLength(50)
            .FileUploadName(FileClass.Image, uploadService)
            .When(u => !string.IsNullOrEmpty(u.Photo));
    }

}
