using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for FriendRequest
/// </summary>
public class FriendRequestConfig : IEntityTypeConfiguration<FriendRequest>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FriendRequest> builder)
    {
        builder.HasQueryFilter(fr => fr.DateDeleted == null);
    }
}
