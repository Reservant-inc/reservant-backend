using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models.Dtos.Thread;

namespace Reservant.Api.Validators.Thread;
public class CreateThreadRequestValidator : AbstractValidator<CreateThreadRequest>
{
    public CreateThreadRequestValidator(ApiDbContext context)
    {
        RuleFor(t => t.Title)
            .NotEmpty()
            .WithMessage("Title cannot be empty.");

        RuleFor(t => t.ParticipantIds)
            .NotEmpty()
            .WithMessage("ParticipantIds cannot be empty.");

        RuleForEach(t => t.ParticipantIds)
            .MustAsync(async (id, cancellation) => await context.Users.AnyAsync(u => u.Id == id))
            .WithMessage("One or more participant IDs are invalid.");
    }
}
