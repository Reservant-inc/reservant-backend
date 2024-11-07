using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for Ingredient
/// </summary>
public class IngrediantConfig : IEntityTypeConfiguration<Ingredient>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.HasOne(o => o.Restaurant)
            .WithMany();
    }
}
