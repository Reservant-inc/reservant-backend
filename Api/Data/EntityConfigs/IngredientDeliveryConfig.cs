using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for IngredientDelivery
/// </summary>
public class IngredientDeliveryConfig : IEntityTypeConfiguration<IngredientDelivery>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<IngredientDelivery> builder)
    {
        builder.HasKey(id => new { id.DeliveryId, id.IngredientId });
    }
}
