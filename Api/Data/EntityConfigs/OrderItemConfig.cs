using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for OrderItem
/// </summary>
public class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => new { oi.MenuItemId, oi.OrderId });
    }
}
