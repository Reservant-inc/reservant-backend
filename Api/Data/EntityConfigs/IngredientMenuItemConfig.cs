using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for IngredientMenuItem
/// </summary>
public class IngredientMenuItemConfig : IEntityTypeConfiguration<IngredientMenuItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<IngredientMenuItem> builder)
    {
        builder.HasKey(im => new { im.MenuItemId, im.IngredientId });
    }
}
