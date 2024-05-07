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

        /// <summary>
        /// File name of the photo
        /// </summary>
        [Required, StringLength(50)]
        public required string PhotoFileName { get; set; }

    }
}
