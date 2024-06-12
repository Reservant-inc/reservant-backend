using FluentValidation;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Services;

namespace Reservant.Api.Validators.Restaurant;

/// <summary>
/// Validator for Review
/// </summary>
public class ReviewValidator : AbstractValidator<Review>
{
    /// <inheritdoc />
    public ReviewValidator()
    {
        RuleFor(x => x.RestaurantId)
            .NotEmpty();

        RuleFor(x => x.AuthorId)
            .Length(36);

        RuleFor(x => x.Stars)
            .NotEmpty()
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(5);

        RuleFor(x => x.CreatedAt)
            .NotEmpty()
            .Must(x=>x<= DateTime.UtcNow);

        RuleFor(x => x.Contents)
            .Length(0, 200);

        RuleFor(x => x.RestaurantResponse)
            .Length(0, 200);

        RuleFor(x => x.IsDeleted)
            .NotEmpty();

    }
}
