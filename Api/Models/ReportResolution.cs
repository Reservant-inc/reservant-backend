using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models
{
    /// <summary>
    /// Represents the resolution details of a report.
    /// </summary>
    public class ReportResolution
    {
        /// <summary>
        /// Comment provided by the support staff resolving the report.
        /// </summary>
        [Required]
        public string SupportComment { get; set; } = null!;

        /// <summary>
        /// The support staff member who resolved the report.
        /// </summary>
        [Required]
        public User ResolvedBy { get; set; } = null!;

        /// <summary>
        /// The date and time when the report was resolved.
        /// </summary>
        public DateTime Date { get; set; }
    }
}