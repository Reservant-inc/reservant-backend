using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos
{
    public class RegisterRestaurantEmployeeRequest
    {
        [Required, StringLength(30)]
        public required string FirstName { get; init; }
        [Required, StringLength(30)]
        public required string LastName { get; init; }
        [Required, StringLength(50)]
        public required string Email { get; init; }
        [Required, StringLength(15), Phone]
        public required string PhoneNumber { get; init; }
    }
}
