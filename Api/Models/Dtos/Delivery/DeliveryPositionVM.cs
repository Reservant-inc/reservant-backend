using System.ComponentModel.DataAnnotations;
using Reservant.Api.Models.Dtos.MenuItem;

namespace Reservant.Api.Models.Dtos.Delivery;

public class DeliveryPositionVM
{

    [Required]
    public required MenuItemVM MenuItem { get; init; }

    [Required] 
    public required int Quantity { get; init; }
}