using Reservant.Api.Models.Enums;

namespace Reservant.Api.Dtos.Order
{
    /// <summary>
    /// Used by restaurant employees to update ordered item's status
    /// </summary>
    public class UpdateOrderItemStatusRequest
    {
        /// <summary>
        /// Ordered item ID
        /// </summary>
        public required int MenuItemId { get; set; }

        /// <summary>
        /// Ordered item status
        /// </summary>
        public required OrderStatus Status { get; set; }
    }
}
