using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for Notification
/// </summary>
public class NotificationConfig : IEntityTypeConfiguration<Notification>
{
    private static readonly Type[] DetailsKinds = typeof(NotificationDetails)
        .Assembly
        .GetTypes()
        .Where(t => t.IsSubclassOf(typeof(NotificationDetails)))
        .ToArray();

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Ignore(n => n.Details);

        builder.Property<Type>("DetailsKind")
            .UsePropertyAccessMode(PropertyAccessMode.Property)
            .HasConversion(type => type.Name, name => DetailsKindToType(name));

        builder.Property<string>("DetailsJson")
            .UsePropertyAccessMode(PropertyAccessMode.Property);
    }

    /// <summary>
    /// Convert details kind name to Type
    /// </summary>
    private static Type DetailsKindToType(string name)
    {
        return DetailsKinds.First(t => t.Name == name);
    }
}
