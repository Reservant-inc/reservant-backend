using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Dtos.Thread;

namespace Reservant.Api.Validators.Thread;

/// <summary>
/// Validator for CreateThreadRequest
/// </summary>
public class CreateThreadRequestValidator : AbstractValidator<CreateThreadRequest>
{
    /// <inheritdoc />
    public CreateThreadRequestValidator(UserManager<Models.User> userManager)
    {
        RuleFor(t => t.Title)
            .NotEmpty()
            .WithMessage("Title cannot be empty.")
            .MaximumLength(40);

        RuleFor(t => t.ParticipantIds)
            .NotEmpty()
            .WithMessage("ParticipantIds cannot be empty.");

        RuleForEach(t => t.ParticipantIds)
            .CustomerExists(userManager);
    }
}
