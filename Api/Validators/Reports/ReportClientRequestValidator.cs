using FluentValidation;
using Reservant.Api.Dtos.Reports;
using Reservant.Api.Models;

namespace Reservant.Api.Validators.Reports;

/// <summary>
/// Validator for <see cref="ReportCustomerRequest"/>
/// </summary>
public class ReportClientRequestValidator : AbstractValidator<ReportCustomerRequest>
{
    /// <inheritdoc />
    public ReportClientRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(Report.MaxDescriptionLength);
    }
}
