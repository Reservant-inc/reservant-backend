namespace Reservant.Api.Dtos.Reports
{
    /// <summary>
    /// ViewModel for a resolved report.
    /// </summary>
    public class ReportResolutionVM
    {
        /// <summary>
        /// The ID of the resolved report.
        /// </summary>
        public int ReportId { get; set; }

        /// <summary>
        /// The comment provided by the support staff.
        /// </summary>
        public string SupportComment { get; set; } = null!;

        /// <summary>
        /// The name of the support staff who resolved the report.
        /// </summary>
        public string ResolvedBy { get; set; } = null!;

        /// <summary>
        /// The date and time when the report was resolved.
        /// </summary>
        public DateTime ResolvedDate { get; set; }

        /// <summary>
        /// A brief description of the report.
        /// </summary>
        public string ReportDescription { get; set; } = null!;
    }
}