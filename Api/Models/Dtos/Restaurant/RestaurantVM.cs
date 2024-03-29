using System.ComponentModel.DataAnnotations;
using Reservant.Api.Models.Dtos.Table;

namespace Reservant.Api.Models.Dtos.Restaurant
{
    public class RestaurantVM
    {
        [Required]
        public required int Id { get; init; }

        [Required, StringLength(50)]
        public required string Name { get; init; }

        [Required, StringLength(70)]
        public required string Address { get; init; }

        [Required]
        public required IEnumerable<TableVM> Tables { get; init; }
    }
}
