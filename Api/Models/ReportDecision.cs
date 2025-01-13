using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models
{
    /// <summary>
    /// Represents the resolution details of a report.
    /// </summary>
    public class ReportDecision
    {
        /// <summary>
        /// Max length of SupportComment
        /// </summary>
        public const int MaxSupportCommentLength = 500;

        /// <summary>
        /// Comment provided by the support staff resolving the report.
        /// </summary>
        [Required, StringLength(MaxSupportCommentLength)]
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

        /// <summary>
        /// Check whether the report resolution was positive for the reporting person.
        /// </summary>
        public bool IsDecisionPositive { get; set; }
    }
}
