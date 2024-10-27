using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for Order
/// </summary>
public class OrderConfig : IEntityTypeConfiguration<Order>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasOne(o => o.AssignedEmployee)
            .WithMany()
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

