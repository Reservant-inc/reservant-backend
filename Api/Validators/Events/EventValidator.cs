using FluentValidation;
using Reservant.Api.Services;

namespace Reservant.Api.Validators.Events;

/// <summary>
/// Validator for Event
/// </summary>
public class EventValidator : AbstractValidator<Models.Event>
{
    /// <inheritdoc />
    public EventValidator(FileUploadService uploadService)
    {
        RuleFor(e => e.Name)
            .MaximumLength(50)
            .NotEmpty()
            .IsValidName();

        RuleFor(x => x.Description)
            .Length(0, 200)
            .When(x => x.Description is not null);

        RuleFor(x => x.MustJoinUntil)
            .LessThan(x => x.Time)
            .WithErrorCode(ErrorCodes.MustJoinUntilMustBeBeforeEventTime);

        RuleFor(x => x.Time)
            .GreaterThan(x => x.MustJoinUntil)
            .WithErrorCode(ErrorCodes.MustJoinUntilMustBeBeforeEventTime);

        RuleFor(m => m.PhotoFileName)
            .FileUploadName(FileClass.Image, uploadService)
            .NotNull();
    }
}
