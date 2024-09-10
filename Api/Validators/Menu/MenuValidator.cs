using FluentValidation;
using Reservant.Api.Data;
using Reservant.Api.Services;

namespace Reservant.Api.Validators.Menu;

/// <summary>
/// Validator for <see cref="Models.Menu"/>
/// </summary>
public class MenuValidator : AbstractValidator<Models.Menu>
{
    /// <inheritdoc />
    public MenuValidator(FileUploadService uploadService, ApiDbContext context)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.AlternateName)
            .NotEmpty()
            .MaximumLength(20)
            .When(x => x.AlternateName is not null);

        RuleFor(x => x.MenuType)
            .IsInEnum();

        RuleFor(x => x.RestaurantId)
            .RestaurantExists(context);

        RuleFor(x => x.PhotoFileName)
            .FileUploadName(FileClass.Image, uploadService);
    }
}
