using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs
{
    /// <summary>
    /// Entity type config for Event
    /// </summary>
    public class EventConfig : IEntityTypeConfiguration<Event>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.HasOne(e => e.Thread)
            .WithMany()
            .HasForeignKey(e => e.ThreadId)
            .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
