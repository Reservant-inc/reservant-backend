using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models;

public class DeliveryPosition
{
    [Key] 
    public int Id { get; set; }

    [Required]
    public MenuItem MenuItem { get; set; }

    [Required] 
    public int Quantity { get; set; }
    
}