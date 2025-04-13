using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for Restaurant
/// </summary>
public class RestaurantConfig : IEntityTypeConfiguration<Restaurant>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.OwnsMany(restaurant => restaurant.Tables);
        builder.OwnsMany(restaurant => restaurant.OpeningHours);
    }
}
