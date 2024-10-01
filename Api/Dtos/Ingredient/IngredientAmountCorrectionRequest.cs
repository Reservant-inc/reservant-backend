namespace Reservant.Api.Dtos.Ingredient
{
    public class IngredientAmountCorrectionRequest
    {
        /// <summary>
        /// Amount after correction
        /// </summary>
        public double NewAmount { get; set; }

        /// <summary>
        /// Reason for changing the amount of ingredient in stock
        /// </summary>
        public string Comment { get; set; } = null!;
    }
}
