using Reservant.Api.Data;

namespace Reservant.Api.Models
{
    /// <summary>
    /// Responsible for storing info about users correcting the amount of ingredients in stock
    /// </summary>
    public class IngredientAmountCorrection : ISoftDeletable
    {
        /// <summary>
        /// Id of the correction
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Id of the ingredient that had the amount in stock corrected
        /// </summary>

        public int IngredientId { get; set; }

        /// <summary>
        /// Navigational property for Ingredient
        /// </summary>
        public Ingredient Ingredient { get; set; } = null!;

        /// <summary>
        /// Amount before correction
        /// </summary>
        public double OldAmount { get; set; }

        /// <summary>
        /// Amount after Correction
        /// </summary>
        public double NewAmount { get; set; }

        /// <summary>
        /// Date and time of the correction
        /// </summary>
        public DateTime CorrectionDate { get; set; }

        /// <summary>
        /// Id of user that made the correction
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Navigational property for the user that made the correction
        /// </summary>
        public User User { get; set; } = null!;

        /// <summary>
        /// Reason for changing the amount of ingredients in stock
        /// </summary>
        public string Comment { get; set; } = null!;

        /// <inheritdoc/>
        public bool IsDeleted { get; set; }
    }
}
