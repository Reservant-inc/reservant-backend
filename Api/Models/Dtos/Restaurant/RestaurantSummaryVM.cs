using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.Restaurant
{
    /// <summary>
    /// Basic restaurant info
    /// </summary>
    public class RestaurantSummaryVM
    {
        /// <summary>
        /// Unique ID
        /// </summary>
        [Required]
        public required int Id { get; init; }

        /// <summary>
        /// Nazwa
        /// </summary>
        /// <example>McJohn's</example>
        [Required, StringLength(50)]
        public required string Name { get; init; }

        /// <summary>
        /// Type of the establishment
        /// </summary>
        public required RestaurantType RestaurantType { get; set; }

        /// <summary>
        /// Adres
        /// </summary>
        ///  <example>ul. Koszykowa 86</example>
        [Required, StringLength(70)]
        public required string Address { get; init; }

        /// <summary>
        /// City of the restaurant
        /// </summary>
        /// <example>Warszawa</example>
        [Required, StringLength(15)]
        public required string City { get; init; }

        /// <summary>
        /// Restaurant group ID
        /// </summary>
        public required int GroupId { get; set; }
    }
}
