﻿using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Visits;

namespace Reservant.Api.Validators.Visits;

/// <summary>
/// Validator for CreateVisitRequest
/// </summary>
public class CreateVisitRequestValidator : AbstractValidator<CreateVisitRequest>
{
    /// <inheritdoc />
    public CreateVisitRequestValidator(UserManager<Models.User> userManager, ApiDbContext dbContext)
    {
        RuleFor(v => v.Date)
            .DateTimeInFuture();

        RuleFor(v => (double) v.NumberOfGuests)
            .GreaterOrEqualToZero();

        RuleFor(v => (double) v.Tip!)
            .GreaterOrEqualToZero();

        RuleFor(v => v.Takeaway)
            .NotNull()
            .WithMessage("Takeaway field must be specified.");

        RuleFor(v => v.RestaurantId)
            .RestaurantExists(dbContext);

        RuleForEach(v => v.ParticipantIds)
            .CustomerExists(userManager);
    }

}
