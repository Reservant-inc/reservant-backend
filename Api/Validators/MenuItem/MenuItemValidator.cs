using FluentValidation;
using Reservant.Api.Services;

namespace Reservant.Api.Validators.MenuItem;

/// <summary>
/// Validator for MenuItem
/// </summary>
public class MenuItemValidator : AbstractValidator<Models.MenuItem>
{
    public MenuItemValidator(FileUploadService uploadService)
    {
        RuleFor(m => m.RestaurantId)
            .NotNull();

        RuleFor(m => m.Price)
            .InclusiveBetween(0, 500);

        RuleFor(m => m.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(m => m.AlternateName)
            .MaximumLength(50);

        RuleFor(m => m.AlcoholPercentage)
            .InclusiveBetween(0, 100)
            .When(m => m.AlcoholPercentage.HasValue);

        RuleFor(m => m.PhotoFileName)
            .FileUploadName(FileClass.Image, uploadService)
            .NotNull();
    }
}
