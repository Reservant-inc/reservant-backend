namespace Reservant.Api.Dtos.Ingredients
{
    /// <summary>
    /// Ingredient sorting order (as in order by)
    /// </summary>
    public enum IngredientSorting
    {
        /// <summary>
        /// By name ascending
        /// </summary>
        NameAsc,

        /// <summary>
        /// By name descending
        /// </summary>
        NameDesc,

        /// <summary>
        /// By amount ascending
        /// </summary>
        AmountAsc,

        /// <summary>
        /// By amount descending
        /// </summary>
        AmountDesc
    }
}