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
    }
}