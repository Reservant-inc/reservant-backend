using FluentValidation;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.Auth;

namespace Reservant.Api.Validators.User;

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
            .MaximumLength(30);

        RuleFor(r => r.LastName)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(r => r.Login)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(r => r.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(50);

        RuleFor(r => r.PhoneNumber)
            .NotEmpty()
            .MaximumLength(15);

        RuleFor(r => r.BirthDate)
            .NotEmpty();

        RuleFor(r => r.Password)
            .NotEmpty()
            .MaximumLength(50);
    }
}
