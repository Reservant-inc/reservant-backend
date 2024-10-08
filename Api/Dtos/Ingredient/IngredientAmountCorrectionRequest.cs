namespace Reservant.Api.Dtos.Ingredient
{
    /// <summary>
    /// Request to update an ingredient's amount
    /// </summary>
    public class IngredientAmountCorrectionRequest
    {
        /// <summary>
        /// Amount after correction
        /// </summary>
        public required double NewAmount { get; set; }

        /// <summary>
        /// Reason for changing the amount of ingredient in stock
        /// </summary>
        public required string Comment { get; set; }
    }
}
