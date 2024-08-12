namespace Reservant.Api.Models.Dtos.Ingredient;

public class IngredientDeliveryVM
{
    public int DeliveryId { get; set; }

    public int IngredientId { get; set; }

    public double AmountOrdered { get; set; }
    public double? AmountDelivered { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public required string StoreName { get; set; }
}
