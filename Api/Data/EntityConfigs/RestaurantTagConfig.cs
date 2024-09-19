using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for RestaurantTag
/// </summary>
public class RestaurantTagConfig : IEntityTypeConfiguration<RestaurantTag>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RestaurantTag> builder)
    {
        builder.HasData([
            new RestaurantTag { Name = "OnSite" },
            new RestaurantTag { Name = "Takeaway" },
            new RestaurantTag { Name = "Asian" },
            new RestaurantTag { Name = "Italian" },
            new RestaurantTag { Name = "Tag1" },
            new RestaurantTag { Name = "Tag2" }
        ]);
    }
}
