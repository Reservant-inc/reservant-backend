using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for Table
/// </summary>
public class TableConfig : IEntityTypeConfiguration<Table>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.HasKey(t => new { t.RestaurantId, t.Id });
    }
}
