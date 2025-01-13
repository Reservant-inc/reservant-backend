using FluentValidation;
using Reservant.Api.Dtos.Reports;
using Reservant.Api.Models;

namespace Reservant.Api.Validators.Reports;

/// <summary>
/// Validator for ResolveReportRequest
/// </summary>
public class ResolveReportRequestValidator : AbstractValidator<ResolveReportRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResolveReportRequestValidator"/> class.
    /// </summary>
    public ResolveReportRequestValidator()
    {
        RuleFor(x => x.SupportComment)
            .NotEmpty()
            .MaximumLength(ReportDecision.MaxSupportCommentLength);
    }
}
