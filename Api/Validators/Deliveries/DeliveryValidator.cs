using FluentValidation;
using Reservant.Api.Models;

namespace Reservant.Api.Validators.Deliveries;

/// <summary>
/// Validator for Delivery
/// </summary>
public class DeliveryValidator : AbstractValidator<Delivery>
{
    /// <inheritdoc/>
    public DeliveryValidator()
    {
        
    }
}
