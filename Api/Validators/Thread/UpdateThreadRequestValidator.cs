using FluentValidation;
using Reservant.Api.Models.Dtos.Thread;

namespace Reservant.Api.Validators.Thread;

/// <summary>
/// Validator for UpdateThreadRequest
/// </summary>
public class UpdateThreadRequestValidator : AbstractValidator<UpdateThreadRequest>
{
    /// <inheritdoc />
    public UpdateThreadRequestValidator()
    {
        RuleFor(t => t.Title)
            .NotEmpty()
            .WithMessage("Title cannot be empty.")
            .MaximumLength(40);
    }
}
