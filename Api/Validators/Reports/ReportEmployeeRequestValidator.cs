using FluentValidation;
using Reservant.Api.Dtos.Reports;
using Reservant.Api.Models;

namespace Reservant.Api.Validators.Reports;

/// <summary>
/// Validator for <see cref="ReportEmployeeRequest"/>
/// </summary>
public class ReportEmployeeRequestValidator : AbstractValidator<ReportEmployeeRequest>
{
    /// <inheritdoc />
    public ReportEmployeeRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(Report.MaxDescriptionLength);
    }
}
