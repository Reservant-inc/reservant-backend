using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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
        [Required]
        public int RestaurantId { get; set; }

        /// <summary>
        /// Menu item details
        /// </summary>
        public List<MenuItemDetails> MenuItems { get; set; } = new List<MenuItemDetails>();

        public class MenuItemDetails
        {
            /// <summary>
            /// Cena
            /// </summary>
            [Range(0, 500)]
            public decimal Price { get; set; }

            /// <summary>
            /// Nazwa
            /// </summary>
            [Required, StringLength(20)]
            public string Name { get; set; }

            /// <summary>
            /// Zawartość alkoholu
            /// </summary>
            [Range(0, 100)]
            public decimal? AlcoholPercentage { get; set; }
        }
    }
}
