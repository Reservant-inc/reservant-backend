using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.FriendRequest;
using Reservant.Api.Validation;
using FluentValidation.Results;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Validators;

namespace Reservant.Api.Services
{
    public class FriendService(ApiDbContext context)
    {

        public async Task<Result<bool>> SendFriendRequestAsync(string senderId, string receiverId)
        {
            var existingRequest = await context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.SenderId == senderId && fr.ReceiverId == receiverId && fr.DateDeleted == null);

            if (existingRequest != null)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(receiverId),
                    ErrorMessage = "Friend request already exists",
                    ErrorCode = ErrorCodes.Duplicate
                };
            }
            
            var receiverExists = await context.Users.AnyAsync(u => u.Id == receiverId);
            if (!receiverExists)
            {
                return new ValidationFailure
                {
                    PropertyName = nameof(receiverId),
                    ErrorMessage = "Receiver user does not exist",
                    ErrorCode = ErrorCodes.NotFound
                };
            }

            var friendRequest = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                DateSent = DateTime.UtcNow
            };

            context.FriendRequests.Add(friendRequest);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task MarkFriendRequestAsReadAsync(string receiverId, string senderId)
        {

            var friendRequest = await context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.SenderId == senderId && fr.ReceiverId == receiverId && fr.DateDeleted == null);

            if (friendRequest != null)
            {
                friendRequest.DateRead = DateTime.UtcNow;
                context.FriendRequests.Update(friendRequest);
                var changes = await context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("Friend request not found");
            }
        }


        public async Task AcceptFriendRequestAsync(string receiverId, string senderId)
        {
            var friendRequest = await context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.SenderId == senderId && fr.ReceiverId == receiverId && fr.DateDeleted == null);
            if (friendRequest != null)
            {
                friendRequest.DateAccepted = DateTime.UtcNow;
                context.FriendRequests.Update(friendRequest);
                var changes = await context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("Friend request not found");
            }
        }



                public async Task DeleteFriendAsync(string receiverId, string senderId)
        {
            var friendRequest = await context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.SenderId == senderId && fr.ReceiverId == receiverId && fr.DateDeleted == null);

            if (friendRequest != null)
            {
                friendRequest.DateDeleted = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        public async Task<Result<Pagination<FriendRequestVM>>> GetFriendsAsync(string userId, int page, int perPage)
        {
            var query = context.FriendRequests
                .Where(fr => fr.ReceiverId == userId && fr.DateAccepted != null && fr.DateDeleted == null)
                .Select(fr => new FriendRequestVM
                {
                    DateSent = fr.DateSent,
                    DateRead = fr.DateRead,
                    DateAccepted = fr.DateAccepted,
                    SenderId = fr.SenderId,
                    ReceiverId = fr.ReceiverId,
                    SenderName = fr.Sender.UserName,
                    ReceiverName = fr.Receiver.UserName
                });

            return await query.PaginateAsync(page, perPage);
        }

        public async Task<Result<Pagination<FriendRequestVM>>> GetIncomingFriendRequestsAsync(string userId, int page, int perPage)
        {
            var query = context.FriendRequests
                .Where(fr => fr.ReceiverId == userId && fr.DateAccepted == null && fr.DateDeleted == null)
                .Select(fr => new FriendRequestVM
                {
                    DateSent = fr.DateSent,
                    DateRead = fr.DateRead,
                    DateAccepted = fr.DateAccepted,
                    SenderId = fr.SenderId,
                    ReceiverId = fr.ReceiverId,
                    SenderName = fr.Sender.UserName,
                    ReceiverName = fr.Receiver.UserName
                });

            return await query.PaginateAsync(page, perPage);
        }

        public async Task<Result<Pagination<FriendRequestVM>>> GetOutgoingFriendRequestsAsync(string userId, int page, int perPage)
        {
            var query = context.FriendRequests
                .Where(fr => fr.SenderId == userId && fr.DateAccepted == null && fr.DateDeleted == null)
                .Select(fr => new FriendRequestVM
                {
                    DateSent = fr.DateSent,
                    DateRead = fr.DateRead,
                    DateAccepted = fr.DateAccepted,
                    SenderId = fr.SenderId,
                    ReceiverId = fr.ReceiverId,
                    SenderName = fr.Sender.UserName,
                    ReceiverName = fr.Receiver.UserName
                });

            return await query.PaginateAsync(page, perPage);
        }
    }
        }
