using FluentValidation;

namespace Reservant.Api.Validators.Wallet;

/// <summary>
/// Validator for PaymentTransaction
/// </summary>
public class PaymentTransactionValidator: AbstractValidator<Models.PaymentTransaction>
{
    /// <inheritdoc />
    public PaymentTransactionValidator()
    {
        RuleFor(p => p.Title)
            .MaximumLength(50);

        RuleFor(p => p.Amount)
            .NotEqual(0);
    }

}
