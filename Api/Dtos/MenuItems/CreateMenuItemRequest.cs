using Reservant.Api.Dtos.Ingredients;

namespace Reservant.Api.Dtos.MenuItems
{
    /// <summary>
    /// DTO containing info about a new menu item
    /// </summary>
    public class CreateMenuItemRequest
    {
        /// <summary>
        /// id of a restaurant
        /// </summary>
        public required int RestaurantId { get; set; }

        /// <summary>
        /// Cena
        /// </summary>
        public required decimal Price { get; set; }

        /// <summary>
        /// Nazwa
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Name in another language
        /// </summary>
        public string? AlternateName { get; set; }

        /// <summary>
        /// Zawartość alkoholu
        /// </summary>
        public decimal? AlcoholPercentage { get; set; }

        /// <summary>
        /// File name of the photo
        /// </summary>
        public required string Photo { get; set; }

        /// <summary>
        /// Ingredients required to make the item
        /// </summary>
        public required List<IngredientRequest> Ingredients { get; set; }

    }
}