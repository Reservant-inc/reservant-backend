using Reservant.Api.Dtos.Restaurants;

namespace Reservant.Api.Dtos.Employments
{
    /// <summary>
    /// Info about a user's employment in a restaurant
    /// </summary>
    public class EmploymentSummaryVM
    {
        /// <summary>
        /// ID of the Employment
        /// </summary>
        public required int EmploymentId { get; init; }

        /// <summary>
        /// summary of the restaurant details
        /// </summary>
        public required RestaurantSummaryVM Restaurant { get; init; }

        /// <summary>
        /// Whether the employee is a backdoor employee
        /// </summary>
        public required bool IsBackdoorEmployee { get; init; }

        /// <summary>
        /// Whether the employee is a hall employee
        /// </summary>
        public required bool IsHallEmployee { get; init; }

        /// <summary>
        /// Date untill the employment was active, null if it still is
        /// </summary>
        public required DateOnly? DateUntill { get; init; }

        /// <summary>
        /// Date from which employment started
        /// </summary>
        public required DateOnly DateFrom { get; init; }
    }
}
