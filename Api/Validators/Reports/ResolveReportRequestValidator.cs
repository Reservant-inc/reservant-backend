using FluentValidation;
using Reservant.Api.Dtos.Reports;

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
            .WithMessage("SupportComment cannot be empty.")
            .MaximumLength(500)
            .WithMessage("SupportComment cannot exceed 500 characters.");
    }
}