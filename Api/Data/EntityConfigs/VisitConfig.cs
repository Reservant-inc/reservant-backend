using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservant.Api.Models;

namespace Reservant.Api.Data.EntityConfigs;

/// <summary>
/// Entity type config for Visit
/// </summary>
public class VisitConfig : IEntityTypeConfiguration<Visit>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.HasOne<Restaurant>(v => v.Restaurant)
            .WithMany()
            .HasForeignKey(v => v.RestaurantId);

        builder.HasOne<User>(v => v.Creator)
            .WithMany(u => u.VisitsCreated);

        builder.HasMany<User>(v => v.Participants)
            .WithMany(u => u.VisitParticipations);

        builder.OwnsOne(v => v.Reservation, reservation =>
        {
            reservation.OwnsOne(v => v.Decision, decision =>
            {
                decision.HasOne(d => d.AnsweredBy)
                    .WithMany();
            });
        });
    }
}
