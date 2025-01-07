using Reservant.Api.Models;

namespace Reservant.Api.Services.ReportServices;

/// <summary>
/// Functional service for assigning reports to customer support agents
/// </summary>
public class AssignReportToAgentService
{
    /// <summary>
    /// Assigns a report to customer support agent
    /// </summary>
    /// <param name="user">Customer support agent that is responsible for processing the report</param>
    /// <param name="report">report to process</param>
    public void AssignReportToAgent(User user, Report report)
    {
        if (user == null || user.ReportProcessings == null || report == null || report.reportProcessings == null)
        {
            return;
        }

        var reportProcessing = new ReportProcessing {
            CustomerSupportEmployee = user,
            Report = report,
            ReportId = report.ReportId,
            CustommerSupportEmployeeId = user.Id,
            ProcessingStartTime = DateTime.Now
        };

        user.ReportProcessings.Add(reportProcessing);

        report.reportProcessings.Add(reportProcessing);
    }
}
