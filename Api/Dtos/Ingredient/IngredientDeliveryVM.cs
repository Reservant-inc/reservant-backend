namespace Reservant.Api.Models.Dtos.Ingredient;

public class IngredientDeliveryVM
{
    public required int DeliveryId { get; set; }

    public required int IngredientId { get; set; }

    public required double AmountOrdered { get; set; }
    public required double? AmountDelivered { get; set; }
    public required DateTime? ExpiryDate { get; set; }
    public required string StoreName { get; set; }
}
