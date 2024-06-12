using FluentValidation;
using Reservant.Api.Data;
using Reservant.Api.Models.Dtos.Review;
using Reservant.Api.Services;

namespace Reservant.Api.Validators.Restaurant;

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
