using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos
{
    public class RestaurantSummaryVM
    {
        [Required]
        public required int Id { get; init; }

        [Required, StringLength(50)]
        public required string Name { get; init; }

        [Required, StringLength(70)]
        public required string Address { get; init; }
    }
}
