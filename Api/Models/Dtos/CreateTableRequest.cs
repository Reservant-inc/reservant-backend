using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos
{
    public class CreateTableRequest
    {
        [Required, Range(1, 10)]
        public required int Capacity { get; init; }
    }
}
