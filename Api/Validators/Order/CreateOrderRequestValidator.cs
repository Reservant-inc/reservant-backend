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
            .VisitExist(context)
            .WithMessage("Visit id must be non-negative")
            .WithErrorCode(ErrorCodes.VisitId);

        RuleFor(o => o.Items)
            .NotEmptyList();

        RuleForEach(o => o.Items)
            .OrderItemExist(context)
            .WithMessage("One or more order items do not exist in the database.")
            .WithErrorCode(ErrorCodes.OrderItemDoesNotExists);

        // Problem z castowaniem doubli (Amount = int), dlatego ChildRules
        RuleForEach(o => o.Items)
            .ChildRules(items =>
            {
                items.RuleFor(i => (double) i.Amount)
                    .GreaterOrEqualToZero()
                    .WithMessage("The amount of each item must be greater than or equal to zero.")
                    .WithErrorCode(ErrorCodes.AmountLessThanOne);
            });
    }
}