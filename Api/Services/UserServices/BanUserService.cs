using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Users;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services.UserServices;

/// <summary>
/// Service responsible for banning users
/// </summary>
public class BanUserService(ApiDbContext dbContext)
{
    /// <summary>
    /// Ban a user
    /// </summary>
    /// <param name="userId">ID of the user to be banned</param>
    /// <param name="dto">Dto of ban</param>
    /// <param name="currentUser">Current user</param>
    [ErrorCode(nameof(userId), ErrorCodes.NotFound)]
    [ErrorCode(nameof(userId), ErrorCodes.InvalidState, "User is already banned")]
    public async Task<Result> BanUser(User currentUser, Guid userId, BanDto dto)
    {
        var user = await FindUserWithId(userId);
        if (user == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(userId),
                ErrorCode = ErrorCodes.NotFound,
            };
        }

        var now = DateTime.UtcNow;
        if (user.IsBannedAt(now))
        {
            return new ValidationFailure
            {
                PropertyName = nameof(userId),
                ErrorCode = ErrorCodes.InvalidState,
            };
        }

        user.BannedUntil = now.Add(dto.TimeSpan);
        await dbContext.SaveChangesAsync();

        return Result.Success;
    }

    private async Task<User?> FindUserWithId(Guid userId)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }
}
