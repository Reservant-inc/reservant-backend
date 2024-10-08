using Reservant.Api.Dtos.Auth;
using Reservant.Api.Dtos.User;

namespace Reservant.Api.Dtos.Ingredient
{
    /// <summary>
    /// Visual model DTO for displaying ingredient amount corrections
    /// </summary>
    public class IngredientAmountCorrectionVM
    {
        /// <summary>
        /// Id of the correction
        /// </summary>
        public required int Id { get; set; }

        /// <summary>
        /// Id of the ingredient
        /// </summary>
        public required int IngredientId { get; set; }

        /// <summary>
        /// Visual model summary of the ingredient
        /// </summary>
        public required IngredientVM IngredientVM { get; set; } = null!;

        /// <summary>
        /// Amount of ingredients in stock before correction
        /// </summary>
        public required double OldAmount { get; set; }

        /// <summary>
        /// Amount of ingredients in stock after correction
        /// </summary>
        public required double NewAmount { get; set; }

        /// <summary>
        /// Date of the correction
        /// </summary>
        public required DateTime CorrectionDate { get; set; }

        /// <summary>
        /// Id of the user that corrected the amount of ingredients
        /// </summary>
        public required Guid UserId { get; set; }

        /// <summary>
        /// Visual model summary of the correction creator
        /// </summary>
        public required UserSummaryVM UserSummaryVM { get; set; } = null!;

        /// <summary>
        /// Reason for changing the amount of ingredient in stock
        /// </summary>
        public required string Comment { get; set; } = null!;
    }
}
