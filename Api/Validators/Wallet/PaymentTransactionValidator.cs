using FluentValidation;
using Reservant.Api.Models;

namespace Reservant.Api.Validators.Wallet;

/// <summary>
/// Validator for PaymentTransaction
/// </summary>
public class PaymentTransactionValidator: AbstractValidator<PaymentTransaction>
{
    /// <inheritdoc />
    public PaymentTransactionValidator()
    {
        RuleFor(p => p.Title)
            .MaximumLength(PaymentTransaction.MaxTitleLength);

        RuleFor(p => p.Amount)
            .NotEqual(0);
    }

}
