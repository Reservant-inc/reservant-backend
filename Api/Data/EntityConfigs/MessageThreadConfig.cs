using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for MessageThread
/// </summary>
public class MessageThreadConfig : IEntityTypeConfiguration<MessageThread>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MessageThread> builder)
    {
        builder.HasOne(mt => mt.Creator).WithMany();
    }
}
