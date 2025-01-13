using FluentValidation;
using Reservant.Api.Dtos.Threads;
using Reservant.Api.Models;

namespace Reservant.Api.Validators.Threads;

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
            .MaximumLength(MessageThread.MaxTitleLength);
    }
}
