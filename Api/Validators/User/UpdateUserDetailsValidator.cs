using FluentValidation;
using Reservant.Api.Dtos.User;
using Reservant.Api.Services;

namespace Reservant.Api.Validators.User;

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
            .IsValidName();

        RuleFor(u => u.LastName)
            .NotEmpty()
            .MaximumLength(30)
            .IsValidName();

        RuleFor(r => r.PhoneNumber)
            .NotEmpty()
            .MaximumLength(15)
            .Matches(@"^\+\d+$");

        RuleFor(r => r.BirthDate)
            .NotEmpty()
            .Must(bd => bd < DateOnly.FromDateTime(DateTime.Now));

        RuleFor(u => u.Photo)
            .MaximumLength(50)
            .FileUploadName(FileClass.Image, uploadService)
            .When(u => !string.IsNullOrEmpty(u.Photo));
    }

}
