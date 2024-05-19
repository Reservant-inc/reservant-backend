namespace Reservant.Api.Models.Dtos.MenuItem
{
    /// <summary>
    /// DTO containing info about a new menu item
    /// </summary>
    public class CreateMenuItemRequest
    {
        /// <summary>
        /// id of a restaurant
        /// </summary>
        public int RestaurantId { get; set; }

        
        /// <summary>
        /// Cena
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Nazwa
         /// </summary>
        public string Name { get; set; }

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
        public required string PhotoFileName { get; set; }

    }
}
