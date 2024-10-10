namespace Reservant.Api.Dtos.Ingredients
{
    /// <summary>
    /// Information about a menu item the ingredient is used in
    /// </summary>
    public class UsedInMenuItemDto
    {
        /// <summary>
        /// id of menu item
        /// </summary>
        public required int MenuItemId { get; set; }

        /// <summary>
        /// amount of used ingredient
        /// </summary>
        public required double AmountUsed { get; set; }
    }
}
