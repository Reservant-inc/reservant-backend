using System.ComponentModel.DataAnnotations;
using Reservant.Api.Models.Dtos.MenuItem;

namespace Reservant.Api.Models.Dtos.Delivery;

public class DeliveryPositionVM
{
    public required int Id { get; set; }

    [Required]
    public required MenuItemVM MenuItem { get; set; }

    [Required] 
    public required int Quantity { get; set; }
}