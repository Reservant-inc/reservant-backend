using System.ComponentModel.DataAnnotations;
using Reservant.Api.Models.Dtos.Table;

namespace Reservant.Api.Models.Dtos.Restaurant
{
    public class CreateRestaurantRequest
    {
        [Required, StringLength(50)]
        public required string Name { get; init; }

        [Required, StringLength(70)]
        public required string Address { get; init; }

        [Required, Length(1, 20)]
        public required List<CreateTableRequest> Tables { get; init; }
    }
}
