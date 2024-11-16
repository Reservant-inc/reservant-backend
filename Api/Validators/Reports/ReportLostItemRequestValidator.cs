using FluentValidation;
using Reservant.Api.Dtos.Reports;
using Reservant.Api.Models;

namespace Reservant.Api.Validators.Reports;

/// <summary>
/// Validator for <see cref="ReportLostItemRequest"/>
/// </summary>
public class ReportLostItemRequestValidator : AbstractValidator<ReportLostItemRequest>
{
    /// <inheritdoc />
    public ReportLostItemRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(Report.MaxDescriptionLength);
    }
}
