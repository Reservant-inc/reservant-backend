namespace Reservant.Api.Models.Dtos.Ingredient
{
    public class MenuItems
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
