using FluentValidation;
using Reservant.Api.Models.Dtos.User;
using Reservant.Api.Services;

namespace Reservant.Api.Validators.User;

/// <summary>
/// Validator for UpdateUserDetailsRequest
/// </summary>
public class UpdateUserDetailsValidator : AbstractValidator<UpdateUserDetailsRequest>
{
    public UpdateUserDetailsValidator(FileUploadService uploadService)
    {
        RuleFor(u => u.FirstName)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(u => u.LastName)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(r => r.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(50);

        RuleFor(r => r.PhoneNumber)
            .NotEmpty()
            .MaximumLength(15);

        RuleFor(r => r.BirthDate)
            .NotEmpty();

        RuleFor(u => u.PhotoFileName)
            .MaximumLength(50)
            .FileUploadName(FileClass.Image, uploadService)
            .When(u => !string.IsNullOrEmpty(u.PhotoFileName));
    }

}
