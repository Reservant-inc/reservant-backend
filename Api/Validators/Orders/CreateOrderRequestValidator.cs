using FluentValidation;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Orders;

namespace Reservant.Api.Validators.Orders;

/// <summary>
/// Validator for CreateOrderRequest
/// </summary>
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    /// <inheritdoc />
    public CreateOrderRequestValidator(ApiDbContext context)
    {
        RuleFor(o => o.VisitId)
            .VisitExist(context);

        RuleFor(o => o.Note)
            .NotEmpty()
            .When(o => o.Note is not null);

        RuleFor(o => o.Items)
            .NotEmptyList();

        RuleForEach(o => o.Items)
            .OrderItemExist(context);

        // Problem z castowaniem doubli (Amount = int), dlatego ChildRules
        RuleForEach(o => o.Items)
            .ChildRules(items =>
            {
                items.RuleFor(i => i.Amount)
                    .GreaterOrEqualToOne();
            });
    }
}
