using FluentValidation;

namespace Reservant.Api.Validators.Review;

/// <summary>
/// Validator for Review
/// </summary>
public class ReviewValidator : AbstractValidator<Models.Review>
{
    /// <inheritdoc />
    public ReviewValidator()
    {
        RuleFor(x => x.Stars)
            .NotEmpty()
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(5);

        RuleFor(x => x.Contents)
            .Length(0, 200);

        RuleFor(x => x.RestaurantResponse)
            .Length(0, 200);

    }
}
