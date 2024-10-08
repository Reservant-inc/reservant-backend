using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for Visit
/// </summary>
public class VisitConfig : IEntityTypeConfiguration<Visit>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.HasOne<Restaurant>(v => v.Restaurant)
            .WithMany()
            .HasForeignKey(v => v.RestaurantId);

        builder.HasOne(v => v.Table)
            .WithMany()
            .HasForeignKey(v => new { v.RestaurantId, v.TableId });
    }
}
