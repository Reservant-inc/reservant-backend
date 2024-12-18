using FluentValidation;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Restaurants;
using Reservant.Api.Models;
using Reservant.Api.Services;

namespace Reservant.Api.Validators.Restaurants;

/// <summary>
/// Validator for RestaurantStatRequestValidator
/// </summary>
public class RestaurantStatRequestValidator : AbstractValidator<RestaurantStatsRequest>
{
    /// <inheritdoc />
    public RestaurantStatRequestValidator(ApiDbContext dbContext)
    {
        RuleFor(r => RestaurantStatsRequest.defaultPopularItemMaxCount)
            .NotEmpty()
            .GreaterThanOrEqualTo(0)
            .WithMessage("defaultPopularItemMaxCount must be 0 or greater.");

        RuleFor(r => r.popularItemMaxCount)
            .NotEmpty()
            .GreaterThanOrEqualTo(0)
            .WithMessage("popularItemMaxCount must be 0 or greater.");

        RuleFor(r => r.dateSince)
            .Must((request, dateSince) => dateSince == null || request.dateTill == null || dateSince <= request.dateTill)
            .WithMessage("dateSince must be either null, lesser than dateTill, or dateTill has to be null.");

        RuleFor(r => r.dateTill)
            .Must((request, dateTill) => dateTill == null || request.dateSince == null || dateTill >= request.dateSince)
            .WithMessage("dateTill must be either null, greater than dateSince, or dateSince has to be null.");
    }
}
