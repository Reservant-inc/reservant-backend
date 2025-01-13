using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Dtos.Reports
{
    /// <summary>
    /// Request to resolve a report.
    /// </summary>
    public class ResolveReportRequest
    {
        /// <summary>
        /// Comment provided by the support staff.
        /// </summary>
        [Required]
        public string SupportComment { get; set; } = null!;

        /// <summary>
        /// Was the reported issue resolved positively
        /// </summary>
        [Required]
        public bool IsResolutionPositive { get; set; } = true;
    }
}