namespace Reservant.Api.Models.Dtos.Order
{
    /// <summary>
    /// Used by restaurant employees to update ordered item's status
    /// </summary>
    public class UpdateOrderItemStatusRequest
    {
        /// <summary>
        /// Ordered item ID
        /// </summary>
        public int MenuItemId { get; set; }

        /// <summary>
        /// Ordered item status
        /// </summary>
        public OrderStatus Status { get; set; }
    }
}
