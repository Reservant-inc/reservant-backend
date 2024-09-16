using Reservant.Api.Dtos.User;

namespace Reservant.Api.Models
{
    public class NotificationNewFriendRequest : NotificationDetails
    {
        public required int FriendRequestId { get; init; }
        public required DateTime DateRequestSend { get; init; }
        public required string SenderId { get; init; }
        public required string ReveiverId { get; init; }
        public required UserDetailsVM SenderDetails { get; init; }
        public required UserDetailsVM ReceiverDetails { get; init; }
    }
}
