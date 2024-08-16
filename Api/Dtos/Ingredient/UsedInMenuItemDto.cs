namespace Reservant.Api.Dtos.Ingredient
{
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
