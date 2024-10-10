using FluentValidation;
using Reservant.Api.Dtos.Reviews;

namespace Reservant.Api.Validators.Reviews;

/// <summary>
/// Validator for CreateReviewRequest
/// </summary>
public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    /// <inheritdoc />
    public CreateReviewRequestValidator()
    {
        RuleFor(r => r.Stars)
            .LessThanOrEqualTo(5)
            .GreaterThanOrEqualTo(1);

        RuleFor(r => r.Contents)
            .MaximumLength(200);
    }
}
