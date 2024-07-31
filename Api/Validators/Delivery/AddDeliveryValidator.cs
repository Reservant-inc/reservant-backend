using FluentValidation;
using Reservant.Api.Data;
using Reservant.Api.Models.Dtos.Delivery;
using Reservant.Api.Models.Dtos.Restaurant;

namespace Reservant.Api.Validators.Delivery;


/// <summary>
/// Validator for AddEmployeeRequest
/// </summary>
public class AddDeliveryValidator : AbstractValidator<DeliveryVM>
{
    
    
    /// <inheritdoc />
    public AddDeliveryValidator(ApiDbContext context) {
        
        RuleFor(delivery => delivery.positions)
            .Must(position => position.Item2 > 0)
            .WithErrorCode(ErrorCodes.DeliveryItemTooLow)
            .WithMessage(ErrorCodes.DeliveryItemTooLow);

    }
}