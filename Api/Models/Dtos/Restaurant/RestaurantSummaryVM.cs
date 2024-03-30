using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.Restaurant
{
    public class RestaurantSummaryVM
    {
        [Required]
        public required int Id { get; init; }

        [Required, StringLength(50)]
        public required string Name { get; init; }

        [Required, StringLength(70)]
        public required string Address { get; init; }

        /// <summary>
        /// City of the restaurant
        /// </summary>
        /// <example>Warszawa</example>
        [Required, StringLength(15)]
        public required string City { get; init; }
    }
}
