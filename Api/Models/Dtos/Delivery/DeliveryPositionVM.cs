using Microsoft.AspNetCore.Antiforgery;
using Reservant.Api.Models.Dtos.MenuItem;
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models.Dtos.Delivery;

public class DeliveryPositionVM
{

    [Required]
    public required MenuItemVM MenuItem { get; init; }

    [Required]
    public required int Quantity { get; set; }
}
