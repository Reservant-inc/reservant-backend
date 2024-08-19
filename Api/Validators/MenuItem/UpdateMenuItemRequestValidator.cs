using FluentValidation;
using Reservant.Api.Dtos.MenuItem;
using Reservant.Api.Services;

namespace Reservant.Api.Validators.MenuItem;

/// <summary>
/// Validator for UpdateMenuItemRequest
/// </summary>
public class UpdateMenuItemRequestValidator : AbstractValidator<UpdateMenuItemRequest>
{
    /// <inheritdoc />
    public UpdateMenuItemRequestValidator(FileUploadService uploadService)
    {
        RuleFor(m => m.Price)
            .InclusiveBetween(0, 500);

        RuleFor(m => m.Name)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(m => m.AlternateName)
            .MaximumLength(20);

        RuleFor(m => m.AlcoholPercentage)
            .InclusiveBetween(0, 100)
            .When(m => m.AlcoholPercentage.HasValue);

        RuleFor(m => m.Photo)
            .NotEmpty()
            .MaximumLength(50)
            .FileUploadName(FileClass.Image, uploadService);
    }
}
