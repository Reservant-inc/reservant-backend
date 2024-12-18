﻿using FluentValidation;
using Reservant.Api.Data;

namespace Reservant.Api.Validators.Visits;

/// <summary>
/// Validator for Visit
/// </summary>
public class VisitValidator : AbstractValidator<Models.Visit>
{
    /// <inheritdoc />
    public VisitValidator(ApiDbContext dbContext)
    {
        RuleFor(v => (double) v.NumberOfGuests)
            .GreaterOrEqualToZero();

        RuleFor(v => (double) v.Tip!)
            .GreaterOrEqualToZero();

        RuleFor(v => v.Takeaway)
            .NotNull()
            .WithMessage("Takeaway field must be specified.");

        RuleFor(v => new Tuple<int, int?>(v.RestaurantId, v.TableId))
            .TableExistsInRestaurant(dbContext);

    }
}
