using FluentValidation;
using Reservant.Api.Data;
using Reservant.Api.Models.Dtos.Order;

namespace Reservant.Api.Validators.Order;

/// <summary>
/// Validator for CreateOrderRequest
/// </summary>
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    /// <inheritdoc />
    public CreateOrderRequestValidator(ApiDbContext context)
    {
        RuleFor(o => o.VisitId)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Visit id must be non-negative")
            .WithErrorCode(ErrorCodes.VisitId);

        RuleFor(o => o.Items)
            .NotEmpty()
            .WithMessage("Items cannot be empty")
            .WithErrorCode(ErrorCodes.EmptyItemList);

        RuleFor(o => o.Items)
            .OrderItemsExist(context);

        RuleForEach(o => o.Items)
            .Must(item => item.Amount > 0)
            .WithMessage("The amount of each item must be greater than 0.")
            .WithErrorCode(ErrorCodes.AmountLessThanOne);
    }
}