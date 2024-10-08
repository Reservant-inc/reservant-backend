using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for User
/// </summary>
public class UserConfig : IEntityTypeConfiguration<User>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Id)
            .HasMaxLength(36);

        builder.Property(u => u.Language)
            .HasConversion<string>(
                culture => culture.ToString(),
                langCode => new CultureInfo(langCode))
            .HasDefaultValue(CultureInfo.InvariantCulture)
            .HasMaxLength(10);

        builder.HasOne<FileUpload>(u => u.Photo)
            .WithOne()
            .HasForeignKey<User>(u => u.PhotoFileName);

        builder.HasMany<Event>(u => u.EventsCreated)
            .WithOne(e => e.Creator)
            .HasForeignKey(e => e.CreatorId);

        builder.HasMany<FileUpload>(u => u.Uploads)
            .WithOne(fu => fu.User);

        builder.HasMany<FriendRequest>(u => u.OutgoingRequests)
            .WithOne(fr => fr.Sender);

        builder.HasMany<FriendRequest>(u => u.IncomingRequests)
            .WithOne(fr => fr.Receiver);

        builder.HasMany(u => u.Threads)
            .WithMany(mt => mt.Participants);
    }
}
