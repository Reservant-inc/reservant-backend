using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for RestaurantPhoto
/// </summary>
public class RestaurantPhotoConfig : IEntityTypeConfiguration<RestaurantPhoto>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RestaurantPhoto> builder)
    {
        builder.HasKey(rp => new { rp.RestaurantId, rp.Order });
    }
}
