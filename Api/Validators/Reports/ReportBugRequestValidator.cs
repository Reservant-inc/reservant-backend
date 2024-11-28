using FluentValidation;
using Reservant.Api.Dtos.Reports;
using Reservant.Api.Models;

namespace Reservant.Api.Validators.Reports;

/// <summary>
/// Validator for <see cref="ReportBugRequest"/>
/// </summary>
public class ReportBugRequestValidator : AbstractValidator<ReportBugRequest>
{
    /// <inheritdoc />
    public ReportBugRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(Report.MaxDescriptionLength);
    }
}
