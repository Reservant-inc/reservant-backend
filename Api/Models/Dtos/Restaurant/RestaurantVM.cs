using System.ComponentModel.DataAnnotations;
using Reservant.Api.Models.Dtos.Table;
using Reservant.Api.Validation;

namespace Reservant.Api.Models.Dtos.Restaurant
{
    public class RestaurantVM
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
        /// <a href="https://pl.wikipedia.org/wiki/Numer_identyfikacji_podatkowej">NIP</a> associated with the restaurant
        /// </summary>
        /// <example>000-00-00-000</example>
        [Required, Nip]
        public required string Nip { get; init; }

        /// <summary>
        /// Adres
        /// </summary>
        /// <example>ul. Koszykowa 86</example>
        [Required, StringLength(70)]
        public required string Address { get; init; }

        /// <summary>
        /// Postal index of the restaurant
        /// </summary>
        /// <example>00-000</example>
        [Required, PostalIndex]
        public required string PostalIndex { get; init; }

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

        /// <summary>
        /// Restaurant group name
        /// </summary>
        /// <example>McJohn's Restaurant Group</example>
        public required string GroupName { get; set; }

        /// <summary>
        /// List of tables in the restaurant
        /// </summary>
        [Required]
        public required IEnumerable<TableVM> Tables { get; init; }
    }
}
