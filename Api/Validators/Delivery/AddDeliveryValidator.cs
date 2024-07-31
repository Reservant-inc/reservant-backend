using FluentValidation;
using Reservant.Api.Data;
using Reservant.Api.Models.Dtos.Delivery;

namespace Reservant.Api.Validators.Delivery;


/// <summary>
/// Validator for AddEmployeeRequest
/// </summary>
public class AddDeliveryValidator : AbstractValidator<CreateDeliveryRequest>
{
    
    
    /// <inheritdoc />
    public AddDeliveryValidator(ApiDbContext context) {
        
        RuleForEach(delivery => delivery.Positions)
            .Must(position => position.Quantity > 0)
            .WithErrorCode(ErrorCodes.DeliveryItemTooLow)
            .WithMessage(ErrorCodes.DeliveryItemTooLow);

    }
}