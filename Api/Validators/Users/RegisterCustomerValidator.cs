using FluentValidation;
using Reservant.Api.Dtos.Auth;

namespace Reservant.Api.Validators.Users;

/// <summary>
/// Validator for RegisterCustomerRequest
/// </summary>
public class RegisterCustomerRequestValidator : AbstractValidator<RegisterCustomerRequest>
{
    /// <inheritdoc />
    public RegisterCustomerRequestValidator()
    {
        RuleFor(r => r.FirstName)
            .NotEmpty()
            .MaximumLength(30)
            .IsValidName();
            

        RuleFor(r => r.LastName)
            .NotEmpty()
            .MaximumLength(30)
            .IsValidName();

        RuleFor(r => r.Login)
            .NotEmpty()
            .MaximumLength(50)
            .IsValidLogin();

        RuleFor(r => r.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(50);

        RuleFor(r => r.PhoneNumber)
            .NotEmpty()
            .MaximumLength(15)
            .IsValidPhoneNumber();

        RuleFor(r => r.BirthDate)
            .NotEmpty()
            .DateInPast();

        RuleFor(r => r.Password)
            .NotEmpty()
            .MaximumLength(50);
    }
}
