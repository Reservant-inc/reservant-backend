﻿using FluentValidation;
using Reservant.Api.Models;
using Reservant.Api.Services;

namespace Reservant.Api.Validators.MenuItems;

/// <summary>
/// Validator for MenuItem
/// </summary>
public class MenuItemValidator : AbstractValidator<MenuItem>
{
    /// <inheritdoc />
    public MenuItemValidator(FileUploadService uploadService)
    {
        RuleFor(m => m.RestaurantId)
            .NotNull();

        RuleFor(m => m.Price)
            .InclusiveBetween(0, 500);

        RuleFor(m => m.Name)
            .NotEmpty()
            .MaximumLength(MenuItem.MaxNameLength);

        RuleFor(m => m.AlternateName)
            .MaximumLength(MenuItem.MaxNameLength);

        RuleFor(m => m.AlcoholPercentage)
            .InclusiveBetween(0, 100)
            .When(m => m.AlcoholPercentage.HasValue);

        RuleFor(m => m.PhotoFileName)
            .FileUploadName(FileClass.Image, uploadService)
            .NotNull();
    }
}
