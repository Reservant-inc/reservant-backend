using FluentValidation;
using Reservant.Api.Services;

namespace Reservant.Api.Validators.User;

/// <summary>
/// Validator for User
/// </summary>
public class UserValidator : AbstractValidator<Models.User>
{
    /// <inheritdoc />
    public UserValidator(FileUploadService uploadService)
    {
        RuleFor(u => u.UserName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(u => u.FirstName)
            .NotEmpty()
            .MaximumLength(30)
            .IsValidName();

        RuleFor(u => u.LastName)
            .NotEmpty()
            .MaximumLength(30)
            .IsValidName();

        RuleFor(u => u.RegisteredAt)
            .NotEmpty();

        RuleFor(u => u.BirthDate)
            .NotEmpty()
            .When(u => u.BirthDate.HasValue)
            .Must(bd => bd < DateOnly.FromDateTime(DateTime.Now) || bd==null);

        RuleFor(u => u.Reputation)
            .GreaterThanOrEqualTo(0)
            .When(u => u.Reputation.HasValue);

        RuleFor(u => u.EmployerId)
            .NotEmpty()
            .When(u => u.EmployerId != null);

        RuleFor(u => u.PhotoFileName)
            .MaximumLength(50)
            .FileUploadName(FileClass.Image, uploadService)
            .When(u => !string.IsNullOrEmpty(u.PhotoFileName));

        
    }
}
