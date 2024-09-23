using Reservant.Api.Dtos.User;

namespace Reservant.Api.Models
{
    public class NotificationFriendRequestAccepted : NotificationDetails
    {
        public required int FriendRequestId { get; init; }
        public required DateTime DateRequestSend { get; init; }
        /// <summary>
        /// ID of the person that send the friend request
        /// </summary>
        public required string SenderId { get; init; }
        /// <summary>
        /// ID of the person that received and accepted the friend request
        /// </summary>
        public required string ReceiverId { get; init; }
        public required UserDetailsVM SenderDetails { get; init; }
        public required UserDetailsVM ReceiverDetails { get; init; }
    }
}
