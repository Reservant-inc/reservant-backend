using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.Table
{
    public class TableVM
    {
        [Required]
        public required int TableId { get; init; }

        [Required, Range(1, 10)]
        public required int Capacity { get; init; }
    }
}
