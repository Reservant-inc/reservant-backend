using Reservant.Api.Dtos.Users;

namespace Reservant.Api.Dtos.Ingredients
{
    /// <summary>
    /// Visual model DTO for displaying ingredient amount corrections
    /// </summary>
    public class IngredientAmountCorrectionVM
    {
        /// <summary>
        /// Id of the correction
        /// </summary>
        public required int CorrectionId { get; set; }

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
        /// The correction author
        /// </summary>
        public required UserSummaryVM User { get; set; } = null!;

        /// <summary>
        /// Reason for changing the amount of ingredient in stock
        /// </summary>
        public required string Comment { get; set; } = null!;
    }
}
