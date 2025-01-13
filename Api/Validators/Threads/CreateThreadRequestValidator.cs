using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Dtos.Threads;
using Reservant.Api.Models;

namespace Reservant.Api.Validators.Threads;

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
            .MaximumLength(MessageThread.MaxTitleLength);

        RuleFor(t => t.ParticipantIds)
            .NotEmpty()
            .WithMessage("ParticipantIds cannot be empty.");

        RuleForEach(t => t.ParticipantIds)
            .CustomerExists(userManager);
    }
}
